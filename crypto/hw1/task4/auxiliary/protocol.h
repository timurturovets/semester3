#pragma once

#include <cstdint>

enum class Operation :uint32_t {
    Encrypt = 1,
    Decrypt = 2,
};