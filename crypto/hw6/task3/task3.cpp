#include "task3.h"
#include "run.h"

#include <algorithm>
#include <iostream>
#include <limits>
#include <stdexcept>

namespace tasks {
    namespace {
        int read_int(const char *prompt) {
            while (true) {
                std::cout << prompt;
                int value = 0;

                if (std::cin >> value) return value;

                std::cin.clear();
                std::cin.ignore(1000000, '\n');
                std::cout << "Некорректный ввод. Введите заново:" << std::endl;
            }
        }

        double read_probability(const char *prompt) {
            while (true) {
                std::cout << prompt;
                double probability = 0.0;

                if (std::cin >> probability) return probability;

                std::cin.clear();
                std::cin.ignore(1000000, '\n');
                std::cout << "Некорректный ввод. Введите заново:" << std::endl;
            }
        }

        std::string read_line(const char *prompt) {
            std::cout << prompt;

            std::string line;
            std::getline(std::cin, line);

            return line;
        }
    }

    int rsa_cipher::modulus_bit_length(std::int64_t n) const {
        if (n <= 1) throw std::invalid_argument("Модуль RSA должен быть больше 1.");

        int bits = 0;
        while (n > 0) {
            n >>= 1LL;
            ++bits;
        }

        return bits;
    }

    int rsa_cipher::plain_block_size_bytes(std::int64_t n) const {
        const int bits = modulus_bit_length(n);
        const int available_bits = bits - 1;
        const int bytes = available_bits / 8;

        if (bytes < 1) return 1;
        return bytes;
    }

    rsa_cipher::encrypted_data rsa_cipher::encrypt(const std::string &text, const rsa_key_generator::public_key &key)
        const {
        if (key.n <= 1) throw std::invalid_argument("Модуль RSA должен быть больше 1.");
        if (key.e <= 1 || key.e >= key.n) throw std::invalid_argument("Показатель e должен быть в диапазоне (1, n).");

        const int block_size = plain_block_size_bytes(key.n);
        encrypted_data result{{}, text.size(), block_size};

        for (std::size_t offset = 0; offset < text.size(); offset += static_cast<std::size_t>(block_size)) {
            const std::size_t chunk = std::min<std::size_t>(static_cast<std::size_t>(block_size), text.size() - offset);

            std::int64_t message_block = 0;
            for (std::size_t i = 0; i < chunk; ++i) {
                const auto byte = static_cast<unsigned char>(text[offset + i]);
                message_block = (message_block << 8LL) | static_cast<std::int64_t>(byte);
            }

            const std::int64_t encrypted_block = pow_mod(message_block, key.e, key.n);
            result.blocks.push_back(encrypted_block);
        }

        return result;
    }

    std::string rsa_cipher::decrypt(const encrypted_data &data, const rsa_key_generator::private_key &key) const {
        if (key.n <= 1) throw std::invalid_argument("Модуль RSA должен быть больше 1.");
        if (key.d <= 1 || key.d >= key.n) throw std::invalid_argument("Показатель d должен быть в диапазоне (1, n).");

        if (data.plain_block_size_bytes <= 0) {
            throw std::invalid_argument("Некорректный размер блока шифрования.");
        }

        std::string result;
        result.reserve(data.source_size);

        const auto block_size = static_cast<std::size_t>(data.plain_block_size_bytes);

        for (std::size_t i = 0; i < data.blocks.size(); ++i) {
            const std::int64_t encrypted_block = data.blocks[i];

            if (encrypted_block < 0 || encrypted_block >= key.n) {
                throw std::invalid_argument("Некорректный блок шифртекста.");
            }

            std::int64_t message_block = pow_mod(encrypted_block, key.d, key.n);
            const std::size_t produced = i * block_size;
            if (produced > data.source_size) {
                throw std::invalid_argument("Некорректный размер исходных данных.");
            }
            const std::size_t remain = data.source_size - produced;
            const std::size_t chunk = std::min(block_size, remain);

            std::string chunk_text(chunk, '\0');
            for (std::size_t j = 0; j < chunk; ++j) {
                const std::size_t pos = chunk - 1 - j;
                chunk_text[pos] = static_cast<char>(message_block & 0xFFLL);
                message_block >>= 8LL;
            }

            result += chunk_text;
        }

        if (result.size() != data.source_size) {
            throw std::runtime_error("Ошибка при восстановлении исходного текста.");
        }

        return result;
    }

    void task3::run(int argc, char **argv) {
        rsa_key_generator key_generator;
        rsa_cipher cipher;

        try {
            const int prime_bits = read_int("Введите битовую длину простых чисел для RSA [4, 31]: ");
            const double min_probability =
                read_probability("Введите минимальную вероятность простоты [0.5, 1): ");

            const auto keys = key_generator.generate_keys(prime_bits, min_probability);

            std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n');
            const std::string source = read_line("Введите текст для шифрования: ");

            const auto encrypted = cipher.encrypt(source, keys.open);
            const std::string decrypted = cipher.decrypt(encrypted, keys.close);

            std::cout << std::endl << "Параметры RSA:" << std::endl;
            std::cout << "n = " << keys.open.n << std::endl;
            std::cout << "e = " << keys.open.e << std::endl;
            std::cout << "d = " << keys.close.d << std::endl;
            std::cout << "Автоматический размер блока открытого текста (байт): "
                << encrypted.plain_block_size_bytes << std::endl;

            std::cout << std::endl << "Шифртекст (блоки):" << std::endl;
            for (std::size_t i = 0; i < encrypted.blocks.size(); ++i) {
                std::cout << encrypted.blocks[i];

                if (i + 1 != encrypted.blocks.size()) std::cout << ' ';
            }
            std::cout << std::endl;

            std::cout << std::endl << "Расшифрованный текст: " << decrypted << std::endl;
        } catch (const std::exception &e) {
            std::cout << "Ошибка: " << e.what() << std::endl;
        }
    }
}
