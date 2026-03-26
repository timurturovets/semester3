#pragma once

#include <cstdint>
#include <string>
#include <vector>

#include "../../hw5/task1/number_theory_service.h"
#include "../task2/task2.h"

namespace tasks {
    class rsa_cipher final : private number_theory_service {
    public:
        struct encrypted_data {
            std::vector<std::int64_t> blocks;
            std::size_t source_size;
            int plain_block_size_bytes;
        };

        encrypted_data encrypt(const std::string &text, const rsa_key_generator::public_key &key) const;
        std::string decrypt(const encrypted_data &data, const rsa_key_generator::private_key &key) const;

    private:
        int modulus_bit_length(std::int64_t n) const;
        int plain_block_size_bytes(std::int64_t n) const;
    };
}
