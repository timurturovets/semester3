#include "run.h"

#include <algorithm>
#include <array>
#include <condition_variable>
#include <cstdint>
#include <filesystem>
#include <fstream>
#include <future>
#include <functional>
#include <iomanip>
#include <iostream>
#include <mutex>
#include <queue>
#include <random>
#include <span>
#include <sstream>
#include <stdexcept>
#include <string>
#include <thread>
#include <type_traits>
#include <vector>

namespace tasks {
    namespace {
        enum class algorithm_kind {
            twofish,
            mars
        };

        enum class cipher_mode {
            ecb,
            cbc,
            pcbc,
            cfb,
            ofb,
            ctr,
            random_delta
        };

        enum class padding_mode {
            zeros,
            pkcs7,
            iso10126,
            ansi_x923
        };

        class block_cipher {
        public:
            virtual ~block_cipher() = default;
            virtual std::string name() const = 0;
            virtual std::size_t block_size() const = 0;
            virtual std::vector<std::uint8_t> encrypt_block(std::span<const std::uint8_t> block) const = 0;
            virtual std::vector<std::uint8_t> decrypt_block(std::span<const std::uint8_t> block) const = 0;
        };

        std::uint32_t rotl32(const std::uint32_t value, const std::uint32_t shift) {
            return (value << (shift & 31U)) | (value >> ((32U - (shift & 31U)) & 31U));
        }

        std::uint32_t rotr32(const std::uint32_t value, const std::uint32_t shift) {
            return (value >> (shift & 31U)) | (value << ((32U - (shift & 31U)) & 31U));
        }

        std::uint32_t load_u32_le(std::span<const std::uint8_t> bytes, const std::size_t offset) {
            return static_cast<std::uint32_t>(bytes[offset]) |
                   (static_cast<std::uint32_t>(bytes[offset + 1]) << 8U) |
                   (static_cast<std::uint32_t>(bytes[offset + 2]) << 16U) |
                   (static_cast<std::uint32_t>(bytes[offset + 3]) << 24U);
        }

        void store_u32_le(std::span<std::uint8_t> bytes, const std::size_t offset, const std::uint32_t value) {
            bytes[offset] = static_cast<std::uint8_t>(value & 0xFFU);
            bytes[offset + 1] = static_cast<std::uint8_t>((value >> 8U) & 0xFFU);
            bytes[offset + 2] = static_cast<std::uint8_t>((value >> 16U) & 0xFFU);
            bytes[offset + 3] = static_cast<std::uint8_t>((value >> 24U) & 0xFFU);
        }

        std::uint64_t load_u64_le(std::span<const std::uint8_t> bytes, const std::size_t offset) {
            std::uint64_t value = 0;
            for (std::size_t i = 0; i < 8; ++i) {
                value |= static_cast<std::uint64_t>(bytes[offset + i]) << (8U * i);
            }
            return value;
        }

        void store_u64_le(std::span<std::uint8_t> bytes, const std::size_t offset, const std::uint64_t value) {
            for (std::size_t i = 0; i < 8; ++i) {
                bytes[offset + i] = static_cast<std::uint8_t>((value >> (8U * i)) & 0xFFU);
            }
        }

        std::vector<std::uint32_t> expand_key_words(std::span<const std::uint8_t> key, const std::size_t count, const std::uint32_t seed) {
            if (key.empty()) {
                throw std::invalid_argument("Ключ не может быть пустым.");
            }

            std::vector<std::uint32_t> source_words((key.size() + 3U) / 4U, 0U);
            for (std::size_t i = 0; i < key.size(); ++i) {
                source_words[i / 4U] |= static_cast<std::uint32_t>(key[i]) << (8U * (i % 4U));
            }

            std::vector<std::uint32_t> schedule(count, 0U);
            std::uint32_t accumulator = seed ^ static_cast<std::uint32_t>(key.size() * 0x01010101ULL);
            for (std::size_t i = 0; i < count; ++i) {
                const std::uint32_t w = source_words[i % source_words.size()];
                const std::uint32_t mix = rotl32(w + static_cast<std::uint32_t>(0x9E3779B9ULL * (i + 1U)) + accumulator, static_cast<std::uint32_t>((i * 7U) % 32U));
                accumulator = rotl32(accumulator ^ mix ^ static_cast<std::uint32_t>(i * 0xA5A5A5A5ULL), static_cast<std::uint32_t>((i * 5U + 3U) % 32U));
                schedule[i] = mix ^ accumulator ^ static_cast<std::uint32_t>(0x3C6EF372ULL * (i + 1U));
            }
            return schedule;
        }

        class twofish_cipher final : public block_cipher {
        public:
            explicit twofish_cipher(const std::vector<std::uint8_t>& key)
                : round_keys_(expand_key_words(key, 36U, 0x54465748U)) {
            }

            std::string name() const override {
                return "Twofish";
            }

            std::size_t block_size() const override {
                return 16U;
            }

            std::vector<std::uint8_t> encrypt_block(std::span<const std::uint8_t> block) const override;
            std::vector<std::uint8_t> decrypt_block(std::span<const std::uint8_t> block) const override;

        private:
            static std::uint32_t g(const std::uint32_t x, const std::uint32_t k) {
                const std::uint32_t a = x ^ k;
                const std::uint32_t b = rotl32(a * 0x9E3779B1U + 0x7F4A7C15U, ((k >> 27U) & 31U) + 1U);
                return b ^ rotr32(b + 0xA5A5A5A5U, 7U);
            }

            std::vector<std::uint32_t> round_keys_;
        };

        class mars_cipher final : public block_cipher {
        public:
            explicit mars_cipher(const std::vector<std::uint8_t>& key)
                : round_keys_(expand_key_words(key, 40U, 0x4D415253U)) {
            }

            std::string name() const override {
                return "MARS";
            }

            std::size_t block_size() const override {
                return 16U;
            }

            std::vector<std::uint8_t> encrypt_block(std::span<const std::uint8_t> block) const override;
            std::vector<std::uint8_t> decrypt_block(std::span<const std::uint8_t> block) const override;

        private:
            static std::uint32_t h(const std::uint32_t x, const std::uint32_t y) {
                const std::uint32_t z = rotl32(x ^ (y + 0xA4A8D57BU), ((x >> 27U) & 31U) + 1U);
                return (z + rotr32(y ^ 0x3C6EF372U, 11U)) ^ rotl32(z, 7U);
            }

            std::vector<std::uint32_t> round_keys_;
        };

        std::vector<std::uint8_t> twofish_cipher::encrypt_block(std::span<const std::uint8_t> block) const {
            if (block.size() != block_size()) {
                throw std::invalid_argument("Некорректный размер блока.");
            }

            std::uint32_t a = load_u32_le(block, 0) ^ round_keys_[0];
            std::uint32_t b = load_u32_le(block, 4) ^ round_keys_[1];
            std::uint32_t c = load_u32_le(block, 8) ^ round_keys_[2];
            std::uint32_t d = load_u32_le(block, 12) ^ round_keys_[3];

            for (std::size_t round = 0; round < 8U; ++round) {
                const std::size_t base = 4U + round * 4U;
                a = rotl32(a + g(d, round_keys_[base]), 1U);
                b = rotl32(b ^ g(a, round_keys_[base + 1U]), 3U);
                c = rotl32(c + g(b, round_keys_[base + 2U]), 6U);
                d = rotl32(d ^ g(c, round_keys_[base + 3U]), 11U);
                std::swap(a, b);
                std::swap(c, d);
            }

            a ^= round_keys_[32];
            b ^= round_keys_[33];
            c ^= round_keys_[34];
            d ^= round_keys_[35];

            std::vector<std::uint8_t> output(block_size());
            store_u32_le(output, 0, a);
            store_u32_le(output, 4, b);
            store_u32_le(output, 8, c);
            store_u32_le(output, 12, d);
            return output;
        }

        std::vector<std::uint8_t> twofish_cipher::decrypt_block(std::span<const std::uint8_t> block) const {
            if (block.size() != block_size()) {
                throw std::invalid_argument("Некорректный размер блока.");
            }

            std::uint32_t a = load_u32_le(block, 0) ^ round_keys_[32];
            std::uint32_t b = load_u32_le(block, 4) ^ round_keys_[33];
            std::uint32_t c = load_u32_le(block, 8) ^ round_keys_[34];
            std::uint32_t d = load_u32_le(block, 12) ^ round_keys_[35];

            for (std::size_t round = 8U; round-- > 0U;) {
                const std::size_t base = 4U + round * 4U;
                std::swap(a, b);
                std::swap(c, d);
                d = rotr32(d, 11U) ^ g(c, round_keys_[base + 3U]);
                c -= g(b, round_keys_[base + 2U]);
                c = rotr32(c, 6U);
                b = rotr32(b, 3U) ^ g(a, round_keys_[base + 1U]);
                a -= g(d, round_keys_[base]);
                a = rotr32(a, 1U);
            }

            a ^= round_keys_[0];
            b ^= round_keys_[1];
            c ^= round_keys_[2];
            d ^= round_keys_[3];

            std::vector<std::uint8_t> output(block_size());
            store_u32_le(output, 0, a);
            store_u32_le(output, 4, b);
            store_u32_le(output, 8, c);
            store_u32_le(output, 12, d);
            return output;
        }

        std::vector<std::uint8_t> mars_cipher::encrypt_block(std::span<const std::uint8_t> block) const {
            if (block.size() != block_size()) {
                throw std::invalid_argument("Некорректный размер блока.");
            }

            std::uint32_t a = load_u32_le(block, 0) + round_keys_[0];
            std::uint32_t b = load_u32_le(block, 4) + round_keys_[1];
            std::uint32_t c = load_u32_le(block, 8) + round_keys_[2];
            std::uint32_t d = load_u32_le(block, 12) + round_keys_[3];

            for (std::size_t round = 0; round < 8U; ++round) {
                const std::size_t base = 4U + round * 4U;
                const std::uint32_t e = h(a + round_keys_[base], b);
                const std::uint32_t f = h(c ^ round_keys_[base + 1U], d);
                b = rotl32(b ^ e, 5U);
                d = rotl32(d + f, 9U);
                a = rotl32(a + h(d, round_keys_[base + 2U]), 13U);
                c = rotl32(c ^ h(b, round_keys_[base + 3U]), 17U);
                const std::uint32_t old_a = a;
                a = b;
                b = c;
                c = d;
                d = old_a;
            }

            a ^= round_keys_[36];
            b ^= round_keys_[37];
            c ^= round_keys_[38];
            d ^= round_keys_[39];

            std::vector<std::uint8_t> output(block_size());
            store_u32_le(output, 0, a);
            store_u32_le(output, 4, b);
            store_u32_le(output, 8, c);
            store_u32_le(output, 12, d);
            return output;
        }

        std::vector<std::uint8_t> mars_cipher::decrypt_block(std::span<const std::uint8_t> block) const {
            if (block.size() != block_size()) {
                throw std::invalid_argument("Некорректный размер блока.");
            }

            std::uint32_t a = load_u32_le(block, 0) ^ round_keys_[36];
            std::uint32_t b = load_u32_le(block, 4) ^ round_keys_[37];
            std::uint32_t c = load_u32_le(block, 8) ^ round_keys_[38];
            std::uint32_t d = load_u32_le(block, 12) ^ round_keys_[39];

            for (std::size_t round = 8U; round-- > 0U;) {
                const std::size_t base = 4U + round * 4U;
                const std::uint32_t previous_a = d;
                const std::uint32_t previous_b = a;
                const std::uint32_t previous_c = b;
                const std::uint32_t previous_d = c;
                c = rotr32(previous_c ^ h(previous_b, round_keys_[base + 3U]), 17U);
                a = rotr32(previous_a - h(previous_d, round_keys_[base + 2U]), 13U);
                d = rotr32(previous_d, 9U) - h(c ^ round_keys_[base + 1U], previous_d);
                b = rotr32(previous_b, 5U) ^ h(a + round_keys_[base], c);
            }

            a -= round_keys_[0];
            b -= round_keys_[1];
            c -= round_keys_[2];
            d -= round_keys_[3];

            std::vector<std::uint8_t> output(block_size());
            store_u32_le(output, 0, a);
            store_u32_le(output, 4, b);
            store_u32_le(output, 8, c);
            store_u32_le(output, 12, d);
            return output;
        }

        std::unique_ptr<block_cipher> make_cipher(const algorithm_kind kind, const std::vector<std::uint8_t>& key) {
            if (kind == algorithm_kind::twofish) {
                return std::make_unique<twofish_cipher>(key);
            }
            return std::make_unique<mars_cipher>(key);
        }

        std::string to_string(const cipher_mode mode) {
            switch (mode) {
                case cipher_mode::ecb: return "ECB";
                case cipher_mode::cbc: return "CBC";
                case cipher_mode::pcbc: return "PCBC";
                case cipher_mode::cfb: return "CFB";
                case cipher_mode::ofb: return "OFB";
                case cipher_mode::ctr: return "CTR";
                case cipher_mode::random_delta: return "Random Delta";
            }
            return "Unknown";
        }

        std::string to_string(const padding_mode padding) {
            switch (padding) {
                case padding_mode::zeros: return "Zeros";
                case padding_mode::pkcs7: return "PKCS7";
                case padding_mode::iso10126: return "ISO 10126";
                case padding_mode::ansi_x923: return "ANSI X9.23";
            }
            return "Unknown";
        }

        std::vector<std::uint8_t> xor_blocks(std::span<const std::uint8_t> lhs, std::span<const std::uint8_t> rhs) {
            if (lhs.size() != rhs.size()) {
                throw std::invalid_argument("Блоки должны иметь одинаковый размер.");
            }
            std::vector<std::uint8_t> result(lhs.size(), 0U);
            for (std::size_t i = 0; i < lhs.size(); ++i) {
                result[i] = lhs[i] ^ rhs[i];
            }
            return result;
        }

        void increment_counter(std::vector<std::uint8_t>& counter, std::span<const std::uint8_t> delta) {
            std::uint16_t carry = 0U;
            for (std::size_t i = 0; i < counter.size(); ++i) {
                const std::uint16_t sum = static_cast<std::uint16_t>(counter[i]) +
                                          static_cast<std::uint16_t>(delta[i]) +
                                          carry;
                counter[i] = static_cast<std::uint8_t>(sum & 0xFFU);
                carry = static_cast<std::uint16_t>(sum >> 8U);
            }
        }

        std::vector<std::uint8_t> make_random_delta(std::span<const std::uint8_t> iv, std::span<const std::uint8_t> key) {
            std::vector<std::uint8_t> delta(iv.begin(), iv.end());
            for (std::size_t i = 0; i < delta.size(); ++i) {
                delta[i] = static_cast<std::uint8_t>((delta[i] ^ key[i % key.size()] ^ static_cast<std::uint8_t>(i * 19U + 1U)) | 1U);
            }
            return delta;
        }

        std::vector<std::uint8_t> deterministic_tail(std::size_t count, const std::uint64_t seed) {
            std::mt19937_64 generator(seed);
            std::vector<std::uint8_t> result(count, 0U);
            for (auto& value : result) {
                value = static_cast<std::uint8_t>(generator() & 0xFFU);
            }
            return result;
        }

        std::vector<std::uint8_t> apply_padding(const std::vector<std::uint8_t>& data, const std::size_t block_size, const padding_mode padding) {
            if (block_size == 0U) {
                throw std::invalid_argument("Размер блока должен быть положительным.");
            }

            const std::size_t remainder = data.size() % block_size;
            const std::size_t pad_size = remainder == 0U ? block_size : block_size - remainder;

            std::vector<std::uint8_t> padded = data;
            padded.resize(data.size() + pad_size, 0U);

            switch (padding) {
                case padding_mode::zeros:
                    break;
                case padding_mode::pkcs7:
                    std::fill(padded.end() - static_cast<std::ptrdiff_t>(pad_size), padded.end(), static_cast<std::uint8_t>(pad_size));
                    break;
                case padding_mode::iso10126: {
                    if (pad_size > 1U) {
                        const auto tail = deterministic_tail(pad_size - 1U, static_cast<std::uint64_t>(data.size() * 1315423911ULL + block_size));
                        std::copy(tail.begin(), tail.end(), padded.end() - static_cast<std::ptrdiff_t>(pad_size));
                    }
                    padded.back() = static_cast<std::uint8_t>(pad_size);
                    break;
                }
                case padding_mode::ansi_x923:
                    padded.back() = static_cast<std::uint8_t>(pad_size);
                    break;
            }

            return padded;
        }

        std::vector<std::uint8_t> trim_to_original_size(const std::vector<std::uint8_t>& data, const std::uint64_t original_size) {
            if (original_size > data.size()) {
                throw std::runtime_error("Поврежденные данные: исходный размер больше расшифрованного буфера.");
            }
            return {data.begin(), data.begin() + static_cast<std::ptrdiff_t>(original_size)};
        }

        std::vector<std::uint8_t> process_blocks_encrypt(const block_cipher& cipher,
                                                         const std::vector<std::uint8_t>& plain,
                                                         const cipher_mode mode,
                                                         const std::vector<std::uint8_t>& iv,
                                                         const std::vector<std::uint8_t>& key) {
            const std::size_t block_size = cipher.block_size();
            if (plain.size() % block_size != 0U) {
                throw std::invalid_argument("Буфер должен быть выровнен по размеру блока.");
            }

            std::vector<std::uint8_t> result;
            result.reserve(plain.size());

            std::vector<std::uint8_t> state = iv;
            std::vector<std::uint8_t> counter = iv;
            const auto delta = make_random_delta(iv, key);
            const std::vector<std::uint8_t> one(block_size, 0U);

            for (std::size_t offset = 0; offset < plain.size(); offset += block_size) {
                const auto block = std::span<const std::uint8_t>(plain.data() + offset, block_size);
                std::vector<std::uint8_t> transformed;

                switch (mode) {
                    case cipher_mode::ecb:
                        transformed = cipher.encrypt_block(block);
                        break;
                    case cipher_mode::cbc: {
                        const auto mixed = xor_blocks(block, state);
                        transformed = cipher.encrypt_block(mixed);
                        state = transformed;
                        break;
                    }
                    case cipher_mode::pcbc: {
                        const auto mixed = xor_blocks(block, state);
                        transformed = cipher.encrypt_block(mixed);
                        state = xor_blocks(block, transformed);
                        break;
                    }
                    case cipher_mode::cfb: {
                        const auto stream = cipher.encrypt_block(state);
                        transformed = xor_blocks(block, stream);
                        state = transformed;
                        break;
                    }
                    case cipher_mode::ofb: {
                        state = cipher.encrypt_block(state);
                        transformed = xor_blocks(block, state);
                        break;
                    }
                    case cipher_mode::ctr: {
                        const auto stream = cipher.encrypt_block(counter);
                        transformed = xor_blocks(block, stream);
                        increment_counter(counter, one);
                        counter[0] = static_cast<std::uint8_t>(counter[0] + 1U);
                        break;
                    }
                    case cipher_mode::random_delta: {
                        const auto stream = cipher.encrypt_block(counter);
                        transformed = xor_blocks(block, stream);
                        increment_counter(counter, delta);
                        break;
                    }
                }

                result.insert(result.end(), transformed.begin(), transformed.end());
            }

            return result;
        }
    }
}
