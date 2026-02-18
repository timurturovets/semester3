#pragma once
#include <cstdint>
#include "protocol.h"

constexpr size_t MAX_SESSIONS = 100;
constexpr size_t MAX_KEY_SIZE = 256;
constexpr size_t MAX_DATA_SIZE = 5 * 1024 * 1024;

struct SessionSlot {
    uint32_t busy;
    uint32_t status;

    Operation op;

    uint32_t key_size;
    uint32_t data_size;

    uint32_t key[MAX_KEY_SIZE];
    uint8_t data[MAX_DATA_SIZE];
};

struct SharedMemory {
    SessionSlot slots[MAX_SESSIONS];
};