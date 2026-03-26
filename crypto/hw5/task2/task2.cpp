#include "task2.h"
#include "run.h"

#include <cmath>
#include <iostream>
#include <random>
#include <stdexcept>

#include "../task1/number_theory_service.h"

namespace tasks {
    int read_choice_task2() {
        while (true) {
            std::cout << std::endl << "Выберите операцию: ";
            int choice = -1;

            if (std::cin >> choice) return choice;

            std::cout << "Некорректный ввод. Введите заново:" << std::endl;
        }
    }

    std::int64_t read_int64(const char *prompt) {
        while (true) {
            std::cout << prompt;
            std::int64_t value = 0;

            if (std::cin >> value) return value;

            std::cout << "Некорректный ввод. Введите заново:" << std::endl;
        }
    }

    double read_probability(const char *prompt) {
        while (true) {
            std::cout << prompt;
            double probability = 0.0;

            if (std::cin >> probability) return probability;

            std::cout << "Некорректный ввод. Введите заново:" << std::endl;
        }
    }

    int calculate_iterations(double min_probability, double witness_probability) {
        if (min_probability < 0.5 || min_probability >= 1.0) {
            throw std::invalid_argument("Минимальная вероятность должна быть в диапазоне [0.5, 1).");
        }

        if (witness_probability <= 0.0 || witness_probability >= 1.0) {
            throw std::invalid_argument("Вероятность обнаружения должна быть в диапазоне (0, 1).");
        }

        if (min_probability <= witness_probability) return 1;

        const double failure_probability = 1.0 - min_probability;
        const double one_iteration_failure = 1.0 - witness_probability;

        const auto iterations = static_cast<int>(
            std::ceil(std::log(failure_probability) / std::log(one_iteration_failure))
        );

        if (iterations < 1) return 1;

        return iterations;
    }

    probabilistic_primality_test::result abstract_probabilistic_primality_test::test(
        std::int64_t value, double min_probability
    ) {
        if (min_probability < 0.5 || min_probability >= 1.0) {
            throw std::invalid_argument("Минимальная вероятность должна быть в диапазоне [0.5, 1).");
        }

        if (value < 2) return {false, 0.0, 0};
        if (value == 2 || value == 3) return {true, 1.0, 0};
        if (value % 2 == 0) return {false, 1.0, 0};

        const double witness_probability = composite_witness_probability();
        const int iterations = calculate_iterations(min_probability, witness_probability);
        const double achieved_probability = 1.0 - std::pow(1.0 - witness_probability, iterations);

        for (int i = 0; i < iterations; ++i) {
            const std::int64_t witness = choose_witness(value);

            if (!run_iteration(value, witness)) return {false, achieved_probability, i + 1};
        }

        return {true, achieved_probability, iterations};
    }

    std::int64_t abstract_probabilistic_primality_test::choose_witness(std::int64_t value) {
        static std::mt19937_64 generator(std::random_device{}());
        std::uniform_int_distribution<std::int64_t> distribution(2, value - 2);

        return distribution(generator);
    }

    bool fermat_primality_test::run_iteration(std::int64_t value, std::int64_t witness) {
        if (number_theory_service::gcd_euclid(witness, value) != 1) return false;
        if (number_theory_service::pow_mod(witness, value - 1, value) != 1) return false;

        return true;
    }

    double fermat_primality_test::composite_witness_probability() const { return 0.5; }

    void task2::run(int argc, char **argv) {
        fermat_primality_test test;

        try {
            const std::int64_t value = read_int64("Введите тестируемое значение n: ");
            const double min_probability = read_probability("Введите минимальную вероятность простоты [0.5, 1): ");

            const auto result = test.test(value, min_probability);

            std::cout << std::endl << "Результат теста Ферма:" << std::endl;
            std::cout << "n = " << value << std::endl;
            std::cout << "Минимальная требуемая вероятность: " << min_probability << std::endl;

            std::cout << "Фактическая вероятность после " << result.iterations << " итераций: "
                << result.achieved_probability << std::endl;

            if (result.is_probably_prime) std::cout << "Число вероятно простое." << std::endl;

            else std::cout << "Число составное." << std::endl;
        } catch (const std::exception &e) {
            std::cout << "Ошибка: " << e.what() << std::endl;
        }
    }
}
