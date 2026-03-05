#include <algorithm>
#include <cmath>
#include <iostream>
#include <limits>
#include <random>
#include <sstream>
#include <string>
#include <vector>

#include "task3.h"
#include "run.h"

namespace tasks {
    bool parse_int(const std::string &text, int &value) {
        try {
            size_t pos = 0;

            long long parsed = std::stoll(text, &pos);
            if (pos != text.size()) return false;

            if (parsed < std::numeric_limits<int>::min()
                || parsed > std::numeric_limits<int>::max()) return false;

            value = static_cast<int>(parsed);

            return true;
        } catch (...) {
            return false;
        }
    }

    bool parse_double(const std::string &text, double &value) {
        try {
            size_t pos = 0;
            value = std::stod(text, &pos);

            return pos == text.size();
        } catch (...) {
            return false;
        }
    }

    bool parse_args(int argc, char **argv, task3_aux::Args &args, std::string &error) {
        std::vector<std::string> arguments;

        for (int i = 1; i < argc; ++i) {
            std::string token = argv[i];

            if (token.rfind("--am-mod=", 0) == 0) {
                int value = 0;
                if (!parse_int(token.substr(9), value)) {
                    error = "Неверное значение --am-mod.";
                    return false;
                }
                args.am_modules = value;

            } else if (token == "--am-mod") {
                if (i + 1 >= argc) {
                    error = "Не задано значение для --am-mod.";
                    return false;
                }

                int value = 0;
                if (!parse_int(argv[++i], value)) {
                    error = "Неверное значение --am-mod.";
                    return false;
                }

                args.am_modules = value;
            } else if (token.rfind("--rm-mod=", 0) == 0) {
                int value = 0;

                if (!parse_int(token.substr(9), value)) {
                    error = "Неверное значение --rm-mod.";
                    return false;
                }

                args.rm_modules = value;
            } else if (token == "--rm-mod") {
                if (i + 1 >= argc) {
                    error = "Не задано значение для --rm-mod.";
                    return false;
                }

                int value = 0;
                if (!parse_int(argv[++i], value)) {
                    error = "Неверное значение --rm-mod.";
                    return false;
                }

                args.rm_modules = value;
            } else if (token.rfind("--seed=", 0) == 0) {
                int value = 0;

                if (!parse_int(token.substr(7), value) || value < 0) {
                    error = "Неверное значение --seed.";
                    return false;
                }

                args.has_seed = true;
                args.seed = static_cast<unsigned int>(value);
            } else if (token == "--seed") {
                if (i + 1 >= argc) {
                    error = "Не задано значение для --seed.";
                    return false;
                }

                int value = 0;

                if (!parse_int(argv[++i], value) || value < 0) {
                    error = "Неверное значение --seed.";
                    return false;
                }

                args.has_seed = true;
                args.seed = static_cast<unsigned int>(value);
            } else if (token.rfind("--max-time=", 0) == 0) {
                int value = 0;

                if (!parse_int(token.substr(11), value)) {
                    error = "Неверное значение --max-time.";
                    return false;
                }

                args.has_max_time = true;
                args.max_time = value;
            } else if (token == "--max-time") {
                if (i + 1 >= argc) {
                    error = "Не задано значение для --max-time.";
                    return false;
                }

                int value = 0;

                if (!parse_int(argv[++i], value)) {
                    error = "Неверное значение --max-time.";
                    return false;
                }

                args.has_max_time = true;
                args.max_time = value;
            } else if (!token.empty() && token[0] == '-') {
                error = "Неизвестный аргумент: " + token;
                return false;
            }

            else arguments.push_back(token);

        }

        if (arguments.size() < 5) {
            error = "Недостаточно аргументов.";
            return false;
        }

        if (!parse_int(arguments[0], args.assembly_machines) ||
            !parse_int(arguments[1], args.recycle_machines) ||
            !parse_int(arguments[2], args.n) ||
            !parse_int(arguments[3], args.t_assembly) ||
            !parse_int(arguments[4], args.t_recycle)
        ) {
            error = "Неверный формат аргументов.";
            return false;
        }

        if (args.n <= 0) {
            error = "n должно быть >= 1.";
            return false;
        }

        size_t expected = 5 + static_cast<size_t>(2 * args.n);
        if (arguments.size() != expected) {
            std::ostringstream oss;
            oss << "Ожидалось " << expected << " аргументов, получено " << arguments.size() << ".";
            error = oss.str();

            return false;
        }

        args.recipe.resize(args.n);
        args.supply.resize(args.n);

        for (int i = 0; i < args.n; ++i) {
            double value = 0.0;
            if (!parse_double(arguments[5 + i], value)) {
                error = "Неверное значение количества компонента в рецепте.";
                return false;
            }

            if (value < 0.0) {
                error = "Количество компонента в рецепте должно быть >= 0.";
                return false;
            }

            args.recipe[i] = value;
        }
        for (int i = 0; i < args.n; ++i) {
            double value = 0.0;

            if (!parse_double(arguments[5 + args.n + i], value)) {
                error = "Неверное значение скорости поступления компонента.";
                return false;
            }

            if (value < 0.0) {
                error = "Скорость поступления компонента должна быть больше нуля.";
                return false;
            }

            args.supply[i] = value;
        }

        return true;
    }

    int effective_time(int base, int modules) {
        double multiplier = 1.0 + 0.1 * static_cast<double>(modules);
        int value = static_cast<int>(std::ceil(static_cast<double>(base) * multiplier));
        return std::max(1, value);
    }

    int sample_quality(int base_quality, int modules, std::mt19937 &rng) {
        if (modules <= 0 || base_quality >= 5) return base_quality;

        const double base_probs[4] = {
            52.0 * std::pow(10.0, -3.0),
            52.0 * std::pow(10.0, -4.0),
            52.0 * std::pow(10.0, -5.0),
            52.0 * std::pow(10.0, -6.0),
        };

        double upgrade_probs[4] = {0.0, 0.0, 0.0, 0.0};
        double sum = 0.0;

        for (int k = 1; k <= 4; ++k) {
            if (base_quality + k <= 5) {
                double p = base_probs[k - 1] * static_cast<double>(modules);
                upgrade_probs[k - 1] = p;
                sum += p;
            }
        }

        double stay_prob = 1.0 - sum;
        if (stay_prob < 0.0) stay_prob = 0.0;

        std::uniform_real_distribution dist(0.0, 1.0);
        double u = dist(rng);

        if (u < stay_prob) return base_quality;

        double cumulative = stay_prob;
        for (int k = 1; k <= 4; ++k) {
            if (upgrade_probs[k - 1] <= 0.0) continue;

            cumulative += upgrade_probs[k - 1];

            if (u < cumulative) return base_quality + k;
        }

        return base_quality;
    }

    int choose_component_quality(const std::vector<std::vector<double>> &components,
                                 const std::vector<double> &recipe) {
        const int n = static_cast<int>(components.size());

        for (int q = 5; q >= 1; --q) {
            bool ok = true;

            for (int i = 0; i < n; ++i) {
                if (components[i][q - 1] + 1e-9 < recipe[i]) {
                    ok = false;
                    break;
                }
            }

            if (ok) return q;
        }

        return 0;
    }

    int choose_recycle_quality(const std::vector<double> &products) {
        for (int q = 4; q >= 1; --q) {
            if (products[q - 1] >= 1.0 - 1e-9) return q;
        }

        return 0;
    }

    void print_times(const std::string &label, const std::vector<int> &times) {
        std::cout << label << " (" << times.size() << "): [";
        for (size_t i = 0; i < times.size(); ++i) {
            if (i > 0) std::cout << ", ";

            std::cout << times[i];
        }

        std::cout << "]" << std::endl;
    }

    void task3::run(int argc, char **argv) {
        task3_aux::Args args;
        std::string error;

        if (!parse_args(argc, argv, args, error)) {
            std::cout << "Некорректные аргументы командной строки." << std::endl
                << "Порядок аргументов: "
                << "<AM> <RM> <n> <t_sb> <t_pr> <q1..qn> <s1..sn> "
                << "[--am-mod M] [--rm-mod M] [--seed S] [--max-time T]" << std::endl;

            return;
        }

        if (args.assembly_machines < 0 || args.recycle_machines < 0 ||
            args.t_assembly <= 0 || args.t_recycle <= 0) {
            std::cout << "Ошибка: некорректные значения параметров." << std::endl;
            return;
        }

        if (args.assembly_machines == 0) {
            std::cout << "Ошибка: количество сборочных автоматов должно быть >= 1." << std::endl;
            return;
        }

        if (args.am_modules < 0 || args.am_modules > 4 || args.rm_modules < 0 || args.rm_modules > 4) {
            std::cout << "Ошибка: число модулей качества должно быть в диапазоне 0..4." << std::endl;
            return;
        }

        if (args.has_max_time && args.max_time < 0) {
            std::cout << "Ошибка: max-time должен быть >= 0." << std::endl;
            return;
        }

        bool upgrades_possible = args.am_modules > 0 || (args.recycle_machines > 0 && args.rm_modules > 0);
        if (!upgrades_possible) {
            std::cout << "Невозможно получить качество выше 1 без модулей качества." << std::endl;
            return;
        }

        std::mt19937 rng;

        if (args.has_seed) rng.seed(args.seed);
        else  rng.seed(std::random_device{}());

        int t_assembly = effective_time(args.t_assembly, args.am_modules);
        int t_recycle = effective_time(args.t_recycle, args.rm_modules);

        std::vector components(args.n, std::vector(5, 0.0));
        std::vector products(5, 0.0);

        std::vector<task3_aux::Job> assembly_jobs(static_cast<size_t>(args.assembly_machines));
        std::vector<task3_aux::Job> recycle_jobs(static_cast<size_t>(args.recycle_machines));

        std::vector<int> times_level3;
        std::vector<int> times_level4;
        std::vector<int> times_level5;

        int legendary_count = 0;
        int time = 0;

        while (legendary_count < 25) {
            for (int i = 0; i < args.n; ++i) {
                components[i][0] += args.supply[i];
            }

            for (auto &job : assembly_jobs) {
                if (job.busy && job.finish_time == time) {
                    int out_quality = sample_quality(job.base_quality, args.am_modules, rng);

                    products[out_quality - 1] += 1.0;

                    if (out_quality == 3) times_level3.push_back(time);
                    else if (out_quality == 4) times_level4.push_back(time);
                    else if (out_quality == 5) {
                        times_level5.push_back(time);
                        legendary_count++;
                    }
                    job.busy = false;
                }
            }

            for (auto &job : recycle_jobs) {
                if (job.busy && job.finish_time == time) {
                    int out_quality = sample_quality(job.base_quality, args.rm_modules, rng);

                    for (int i = 0; i < args.n; ++i) {
                        components[i][out_quality - 1] += 0.25 * args.recipe[i];
                    }

                    job.busy = false;
                }
            }

            if (legendary_count >= 25) break;

            for (auto &job : assembly_jobs) {
                if (job.busy) continue;

                int quality = choose_component_quality(components, args.recipe);
                if (quality == 0) break;

                for (int i = 0; i < args.n; ++i) {
                    components[i][quality - 1] -= args.recipe[i];
                }

                job.busy = true;
                job.finish_time = time + t_assembly;
                job.base_quality = quality;
            }

            for (auto &job : recycle_jobs) {
                if (job.busy) continue;

                int quality = choose_recycle_quality(products);

                if (quality == 0) break;

                products[quality - 1] -= 1.0;

                job.busy = true;
                job.finish_time = time + t_recycle;
                job.base_quality = quality;
            }

            time++;

            if (args.has_max_time && time > args.max_time) {
                std::cout << "Достигнут предел времени моделирования: " << args.max_time << std::endl;
                break;
            }
        }

        std::cout << "Моделирование завершено. Текущее время: " << time << std::endl;
        std::cout << "Построено легендарных продуктов: " << legendary_count << std::endl;
        print_times("Времена производства уровня 3", times_level3);
        print_times("Времена производства уровня 4", times_level4);
        print_times("Времена производства уровня 5", times_level5);
    }
}
