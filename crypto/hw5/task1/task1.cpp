#include "task1.h"
#include "run.h"

#include <cmath>
#include <iostream>
#include <limits>
#include <stdexcept>
#include <utility>

namespace tasks {
    int read_choice() {
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

    std::uint64_t abs_u64(std::int64_t value) {
        if (value >= 0) return static_cast<std::uint64_t>(value);

        return static_cast<std::uint64_t>(-(value + 1)) + 1ULL;
    }

    std::int64_t task1_service::gcd_euclid(std::int64_t a, std::int64_t b) {
        std::uint64_t x = abs_u64(a);
        std::uint64_t y = abs_u64(b);

        while (y != 0) {
            const std::uint64_t remainder = x % y;
            x = y;
            y = remainder;
        }

        return static_cast<std::int64_t>(x);
    }

    task1_service::extended_gcd_result task1_service::gcd_euclid_extended(std::int64_t a, std::int64_t b) {
        std::int64_t old_r = a;
        std::int64_t r = b;
        std::int64_t old_x = 1;
        std::int64_t x = 0;
        std::int64_t old_y = 0;
        std::int64_t y = 1;

        while (r != 0) {
            const std::int64_t q = old_r / r;

            const std::int64_t next_r = old_r - q * r;
            old_r = r;
            r = next_r;

            const std::int64_t next_x = old_x - q * x;
            old_x = x;
            x = next_x;

            const std::int64_t next_y = old_y - q * y;
            old_y = y;
            y = next_y;
        }

        if (old_r < 0) {
            old_r = -old_r;
            old_x = -old_x;
            old_y = -old_y;
        }

        return {old_r, old_x, old_y};
    }

    std::int64_t task1_service::pow_mod(std::int64_t base, std::int64_t exponent, std::int64_t mod) {
        return mod_pow(base, exponent, mod);
    }

    std::int64_t task1_service::mod_inverse(std::int64_t value, std::int64_t n) {
        if (n <= 1) throw std::invalid_argument("Для обратного элемента n должно быть больше 1.");

        const auto result = gcd_euclid_extended(value, n);

        if (result.gcd != 1) throw std::invalid_argument("Обратный элемент не существует (НОД(value, n) != 1).");

        return normalize_mod(result.x, n);
    }

    std::int64_t task1_service::euler_phi_definition(std::int64_t n) {
        if (n <= 0) throw std::invalid_argument("Для функции Эйлера n должно быть положительным.");

        std::int64_t result = 0;

        for (std::int64_t k = 1; k <= n; ++k) {
            if (gcd_euclid(k, n) == 1) ++result;
        }

        return result;
    }

    std::int64_t task1_service::euler_phi_factorization(std::int64_t n) {
        if (n <= 0) throw std::invalid_argument("Для функции Эйлера n должно быть положительным.");

        std::int64_t result = n;
        std::int64_t x = n;

        for (std::int64_t p = 2; p <= x / p; ++p) {
            if (x % p != 0) continue;

            while (x % p == 0) x /= p;

            result -= result / p;
        }

        if (x > 1) result -= result / x;

        return result;
    }

    std::int64_t task1_service::euler_phi_fourier(std::int64_t n) {
        if (n <= 0) throw std::invalid_argument("Для функции Эйлера n должно быть положительным.");

        const long double pi = std::acos(-1.0L);
        long double sum = 0.0L;

        for (std::int64_t k = 1; k <= n; ++k) {
            const long double angle = 2.0L * pi * static_cast<long double>(k) / static_cast<long double>(n);
            sum += static_cast<long double>(gcd_euclid(k, n)) * std::cos(angle);
        }

        return std::llround(sum);
    }

    int task1_service::legendre_symbol(std::int64_t a, std::int64_t p) {
        if (!is_odd_prime(p)) throw std::invalid_argument("Для символа Лежандра p должно быть нечетным простым.");

        const std::int64_t reduced_a = normalize_mod(a, p);

        if (reduced_a == 0) return 0;

        const std::int64_t value = mod_pow(reduced_a, (p - 1) / 2, p);

        if (value == 1) return 1;

        if (value == p - 1) return -1;

        return 0;
    }

    int task1_service::jacobi_symbol(std::int64_t a, std::int64_t n) {
        if (n <= 1 || n % 2 == 0) throw std::invalid_argument("Для символа Якоби n должно быть нечетным и больше 1.");

        int result = 1;
        a = normalize_mod(a, n);

        while (a != 0) {
            while (a % 2 == 0) {
                a /= 2;
                const std::int64_t n_mod_8 = n % 8;

                if (n_mod_8 == 3 || n_mod_8 == 5) result = -result;
            }

            std::swap(a, n);

            if (a % 4 == 3 && n % 4 == 3) result = -result;

            a = normalize_mod(a, n);
        }

        return n == 1 ? result : 0;
    }

    std::int64_t task1_service::normalize_mod(std::int64_t value, std::int64_t mod) {
        if (mod <= 0) throw std::invalid_argument("Модуль должен быть положительным.");

        value %= mod;

        if (value < 0) value += mod;

        return value;
    }

    std::int64_t task1_service::multiply_mod(std::int64_t left, std::int64_t right, std::int64_t mod) {
        const auto m = static_cast<std::uint64_t>(mod);
        auto a = static_cast<std::uint64_t>(normalize_mod(left, mod));
        auto b = static_cast<std::uint64_t>(normalize_mod(right, mod));

        std::uint64_t result = 0;

        while (b != 0) {
            if (b & 1ULL) {
                if (result >= m - a) result = result - (m - a);
                else result += a;
            }

            b >>= 1ULL;

            if (b == 0) break;

            if (a >= m - a) a = a - (m - a);
            else a += a;
        }

        return static_cast<std::int64_t>(result);
    }

    std::int64_t task1_service::mod_pow(std::int64_t base, std::int64_t exponent, std::int64_t mod) {
        if (mod <= 0) throw std::invalid_argument("Модуль должен быть положительным.");

        if (exponent < 0) throw std::invalid_argument("Показатель степени должен быть неотрицательным.");

        std::int64_t result = 1 % mod;
        base = normalize_mod(base, mod);

        while (exponent > 0) {
            if (exponent & 1LL) result = multiply_mod(result, base, mod);

            base = multiply_mod(base, base, mod);
            exponent >>= 1LL;
        }

        return result;
    }

    bool task1_service::is_odd_prime(std::int64_t value) {
        if (value <= 2) return false;

        if (value % 2 == 0) return false;

        for (std::int64_t divisor = 3; divisor <= value / divisor; divisor += 2) {
            if (value % divisor == 0) return false;
        }

        return true;
    }

    void task1::run(int argc, char **argv) {
        while (true) {
            std::cout << "1. Вычислить символ Лежандра" << std::endl;
            std::cout << "2. Вычислить символ Якоби" << std::endl;
            std::cout << "3. Вычислить НОД (алгоритм Евклида)" << std::endl;
            std::cout << "4. Вычислить НОД и найти коэффициенты Безу (расширенный алгоритм Евклида)" << std::endl;
            std::cout << "5. Возвести в степень по модулю" << std::endl;
            std::cout << "6. Найти мультипликативный обратный элемент в Z_n" << std::endl;
            std::cout << "7. Вычислить функцию Эйлера φ(n)" << std::endl;
            std::cout << "0. В меню" << std::endl;

            const int choice = read_choice();

            if (choice == 0) return;

            try {
                if (choice == 1) {
                    const std::int64_t a = read_int64("Введите a: ");
                    const std::int64_t p = read_int64("Введите p (нечетное простое): ");

                    const int value = task1_service::legendre_symbol(a, p);

                    std::cout << "Символ Лежандра (" << a << "/" << p << ") = " << value << std::endl;

                    continue;
                }

                if (choice == 2) {
                    const std::int64_t a = read_int64("Введите a: ");
                    const std::int64_t n = read_int64("Введите n (нечетное > 1): ");

                    const int value = task1_service::jacobi_symbol(a, n);

                    std::cout << "Символ Якоби (" << a << "/" << n << ") = " << value << std::endl;

                    continue;
                }

                if (choice == 3) {
                    const std::int64_t a = read_int64("Введите первое целое число: ");
                    const std::int64_t b = read_int64("Введите второе целое число: ");

                    const std::int64_t value = task1_service::gcd_euclid(a, b);

                    std::cout << "НОД(" << a << ", " << b << ") = " << value << std::endl;

                    continue;
                }

                if (choice == 4) {
                    const std::int64_t a = read_int64("Введите первое целое число: ");
                    const std::int64_t b = read_int64("Введите второе целое число: ");

                    const auto result = task1_service::gcd_euclid_extended(a, b);

                    std::cout << "НОД(" << a << ", " << b << ") = " << result.gcd << std::endl;
                    std::cout << "Коэффициенты Безу: x = " << result.x << ", y = " << result.y << std::endl;
                    std::cout << "Соотношение Безу: " << a << " * " << result.x << " + " << b << " * " << result.y << " = " << result.gcd << std::endl;

                    continue;
                }

                if (choice == 5) {
                    const std::int64_t base = read_int64("Введите основание: ");
                    const std::int64_t exponent = read_int64("Введите показатель степени (>= 0): ");
                    const std::int64_t mod = read_int64("Введите модуль (> 0): ");

                    const std::int64_t value = task1_service::pow_mod(base, exponent, mod);

                    std::cout << "(" << base << "^" << exponent << ") mod " << mod << " = " << value << std::endl;

                    continue;
                }

                if (choice == 6) {
                    const std::int64_t value = read_int64("Введите значение из Z_n: ");
                    const std::int64_t n = read_int64("Введите n (> 1): ");

                    const std::int64_t inverse = task1_service::mod_inverse(value, n);

                    std::cout << "Обратный элемент: " << inverse << std::endl;
                    std::cout << "Проверка: (" << value << " * " << inverse << ") mod " << n << " = 1" << std::endl;

                    continue;
                }

                if (choice == 7) {
                    const std::int64_t n = read_int64("Введите n (> 0): ");

                    std::cout << "1. По определению" << std::endl;
                    std::cout << "2. По факторизации (основная теорема арифметики)" << std::endl;
                    std::cout << "3. Через сумму с cos (дискретное преобразование Фурье)" << std::endl;

                    const int method = read_choice();

                    if (method == 1) {
                        const std::int64_t value = task1_service::euler_phi_definition(n);
                        std::cout << "φ(" << n << ") = " << value << std::endl;
                        continue;
                    }

                    if (method == 2) {
                        const std::int64_t value = task1_service::euler_phi_factorization(n);
                        std::cout << "φ(" << n << ") = " << value << std::endl;
                        continue;
                    }

                    if (method == 3) {
                        const std::int64_t value = task1_service::euler_phi_fourier(n);
                        std::cout << "φ(" << n << ") = " << value << std::endl;
                        continue;
                    }

                    std::cout << "Неизвестный метод." << std::endl;
                    continue;
                }

                std::cout << "Неизвестный пункт меню." << std::endl;
            } catch (const std::exception &e) {
                std::cout << "Ошибка: " << e.what() << std::endl;
            }
        }
    }
}
