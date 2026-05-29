#include "run.h"
#include "task1.h"

#include <algorithm>
#include <array>
#include <iostream>
#include <limits>
#include <stdexcept>

namespace tasks {
    uint64_t MillerRabin::mulMod(const uint64_t a, const uint64_t b, const uint64_t modulus) {
        return static_cast<uint64_t>((static_cast<unsigned __int128>(a) * static_cast<unsigned __int128>(b)) % modulus);
    }

    uint64_t MillerRabin::modPow(uint64_t base, uint64_t exponent, const uint64_t modulus) {
        if (modulus == 0) {
            throw std::invalid_argument("Модуль должен быть положительным");
        }
        if (modulus == 1) {
            return 0;
        }

        uint64_t result = 1;
        base %= modulus;

        while (exponent > 0) {
            if ((exponent & 1ULL) != 0ULL) {
                result = mulMod(result, base, modulus);
            }
            exponent >>= 1;
            base = mulMod(base, base, modulus);
        }

        return result;
    }

    uint64_t MillerRabin::randomInRange(const uint64_t left, const uint64_t right, std::mt19937_64 &generator) {
        if (left > right) {
            throw std::invalid_argument("Некорректный диапазон для генерации случайного числа");
        }

        std::uniform_int_distribution<uint64_t> distribution(left, right);
        return distribution(generator);
    }

    bool MillerRabin::isProbablePrime(const uint64_t n, const int rounds, std::mt19937_64 &generator) {
        if (n < 2) {
            return false;
        }

        static constexpr std::array<int, 12> smallPrimes = {2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37};
        for (const int prime: smallPrimes) {
            if (n == prime) {
                return true;
            }
            if (n % prime == 0) {
                return false;
            }
        }

        if ((n & 1ULL) == 0ULL) {
            return false;
        }

        uint64_t d = n - 1;
        unsigned int r = 0;
        while ((d & 1ULL) == 0ULL) {
            d >>= 1;
            ++r;
        }

        for (int i = 0; i < rounds; ++i) {
            const uint64_t a = randomInRange(2, n - 2, generator);
            uint64_t x = modPow(a, d, n);

            if (x == 1 || x == n - 1) {
                continue;
            }

            bool witnessFound = true;
            for (unsigned int j = 1; j < r; ++j) {
                x = mulMod(x, x, n);
                if (x == n - 1) {
                    witnessFound = false;
                    break;
                }
            }

            if (witnessFound) {
                return false;
            }
        }

        return true;
    }

    uint64_t MillerRabin::generatePrime(const unsigned int bitLength, const int rounds, std::mt19937_64 &generator) {
        if (bitLength < 2 || bitLength > 31) {
            throw std::invalid_argument("Длина простого числа в битах должна быть не меньше 2");
        }

        const uint64_t lowerBound = 1ULL << (bitLength - 1);
        const uint64_t upperBound = (1ULL << bitLength) - 1ULL;

        while (true) {
            uint64_t candidate = randomInRange(lowerBound, upperBound, generator);
            candidate |= 1ULL;

            const uint64_t mod4 = candidate % 4;
            if (mod4 != 3) {
                candidate += (3 - mod4);
            }

            if (candidate <= upperBound && isProbablePrime(candidate, rounds, generator)) {
                return candidate;
            }
        }
    }

    uint64_t RabinCipher::modInverse(const uint64_t a, const uint64_t modulus) {
        int64_t t = 0;
        int64_t newT = 1;
        int64_t r = static_cast<int64_t>(modulus);
        int64_t newR = static_cast<int64_t>(a % modulus);

        while (newR != 0) {
            const int64_t quotient = r / newR;

            const int64_t tempT = t - quotient * newT;
            t = newT;
            newT = tempT;

            const int64_t tempR = r - quotient * newR;
            r = newR;
            newR = tempR;
        }

        if (r != 1) {
            throw std::runtime_error("Обратный элемент не существует");
        }

        if (t < 0) {
            t += static_cast<int64_t>(modulus);
        }

        return static_cast<uint64_t>(t);
    }

    uint64_t RabinCipher::encodeByte(const uint8_t value) {
        return static_cast<unsigned int>(value) * 257;
    }

    bool RabinCipher::decodeByte(const uint64_t value, uint8_t &decoded) {
        if (value > 65535ULL) {
            return false;
        }

        const auto reduced = static_cast<unsigned int>(value);
        const unsigned int low = reduced & 0xFFu;
        const unsigned int high = (reduced >> 8) & 0xFFu;

        if (low != high) {
            return false;
        }

        decoded = static_cast<uint8_t>(low);
        return true;
    }

    RabinKeyPair RabinCipher::generateKeyPair(const unsigned int primeBitLength, const int rounds,
                                              std::mt19937_64 &generator) {
        if (primeBitLength < 16) {
            throw std::invalid_argument("Длина простого числа должна быть не меньше 16 бит для кодирования байтов");
        }

        RabinKeyPair keyPair;
        keyPair.p = MillerRabin::generatePrime(primeBitLength, rounds, generator);
        do {
            keyPair.q = MillerRabin::generatePrime(primeBitLength, rounds, generator);
        } while (keyPair.q == keyPair.p);

        keyPair.n = keyPair.p * keyPair.q;
        return keyPair;
    }

    std::vector<uint64_t> RabinCipher::encrypt(const std::vector<uint8_t> &data, const uint64_t n) {
        std::vector<uint64_t> ciphertext;
        ciphertext.reserve(data.size());

        for (const uint8_t byte: data) {
            const uint64_t m = encodeByte(byte);
            ciphertext.push_back(MillerRabin::mulMod(m, m, n));
        }

        return ciphertext;
    }

    std::vector<uint64_t> RabinCipher::squareRootsModComposite(const uint64_t value, const RabinKeyPair &keyPair) {
        const uint64_t p = keyPair.p;
        const uint64_t q = keyPair.q;
        const uint64_t n = keyPair.n;

        const uint64_t mp = MillerRabin::modPow(value, (p + 1) / 4, p);
        const uint64_t mq = MillerRabin::modPow(value, (q + 1) / 4, q);

        const uint64_t yP = static_cast<uint64_t>((static_cast<unsigned __int128>(q) * modInverse(q, p)) % n);
        const uint64_t yQ = static_cast<uint64_t>((static_cast<unsigned __int128>(p) * modInverse(p, q)) % n);

        const uint64_t term1 = static_cast<uint64_t>(
            (static_cast<unsigned __int128>(yP) * static_cast<unsigned __int128>(mp)) % n);
        const uint64_t term2 = static_cast<uint64_t>(
            (static_cast<unsigned __int128>(yQ) * static_cast<unsigned __int128>(mq)) % n);

        const uint64_t r1 = (term1 + term2) % n;
        const uint64_t r2 = (n - r1) % n;
        const uint64_t r3 = (term1 + n - term2) % n;
        const uint64_t r4 = (n - r3) % n;

        return {r1, r2, r3, r4};
    }

    std::vector<uint8_t> RabinCipher::decrypt(const std::vector<uint64_t> &ciphertext, const RabinKeyPair &keyPair) {
        std::vector<uint8_t> plaintext;
        plaintext.reserve(ciphertext.size());

        for (const uint64_t value: ciphertext) {
            const std::vector<uint64_t> roots = squareRootsModComposite(value, keyPair);

            bool decodedRoot = false;
            for (const uint64_t root: roots) {
                uint8_t byte = 0;
                if (decodeByte(root, byte)) {
                    plaintext.push_back(byte);
                    decodedRoot = true;
                    break;
                }
            }

            if (!decodedRoot) {
                throw std::runtime_error("Не удалось выбрать единственный корень открытого текста");
            }
        }

        return plaintext;
    }

    std::vector<uint8_t> ByteSequence::fromString(const std::string &text) {
        return {text.begin(), text.end()};
    }

    std::string ByteSequence::toString(const std::vector<uint8_t> &bytes) {
        return {bytes.begin(), bytes.end()};
    }

    void task1::run(int argc, char **argv) {
        (void) argc;
        (void) argv;

        if (std::cin.peek() == '\n') {
            std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n');
        }

        std::string input;
        std::cout << "Введите строку для шифрования: ";
        std::getline(std::cin, input);

        std::random_device randomDevice;
        std::mt19937_64 generator(randomDevice());

        constexpr unsigned int primeBitLength = 24;
        constexpr int rounds = 24;
        const RabinKeyPair keyPair = RabinCipher::generateKeyPair(primeBitLength, rounds, generator);

        const std::vector<uint8_t> source = ByteSequence::fromString(input);
        const std::vector<uint64_t> encrypted = RabinCipher::encrypt(source, keyPair.n);
        const std::vector<uint8_t> decrypted = RabinCipher::decrypt(encrypted, keyPair);
        const std::string output = ByteSequence::toString(decrypted);

        std::cout << std::endl;
        std::cout << "Открытый текст (байты): ";
        for (const uint8_t byte: source) {
            std::cout << static_cast<unsigned int>(byte) << ' ';
        }
        std::cout << std::endl;

        std::cout << "p = " << keyPair.p << std::endl;
        std::cout << "q = " << keyPair.q << std::endl;
        std::cout << "n = " << keyPair.n << std::endl;

        std::cout << "Шифртекст: ";
        for (const uint64_t block: encrypted) {
            std::cout << block << ' ';
        }
        std::cout << std::endl;

        std::cout << "Расшифрованный текст (байты): ";
        for (const uint8_t byte: decrypted) {
            std::cout << static_cast<unsigned int>(byte) << ' ';
        }
        std::cout << std::endl;

        std::cout << "Расшифрованная строка: " << output << std::endl;
    }
}