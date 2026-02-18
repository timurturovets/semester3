#pragma once

#include <windows.h>
#include <cstdint>

constexpr size_t MAX_DATA_SIZE = 10 * 1024 * 1024;

enum class Operation : uint32_t {
    Encrypt = 1,
    Decrypt = 2
};

struct Request {
    Operation op;
    uint32_t key_size;
    uint32_t data_size;
    uint8_t payload[];
};

struct Response {
    uint32_t data_size;
    uint8_t data[];
};

constexpr size_t SHM_SIZE = sizeof(Request) + MAX_DATA_SIZE
                          + sizeof(Response) + MAX_DATA_SIZE;

constexpr wchar_t SHM_NAME[]      = L"Global\\RC4_SharedMemory";
constexpr wchar_t SLOT_SEM_NAME[] = L"Global\\RC4_SlotSemaphore";
constexpr wchar_t REQ_SEM_NAME[]  = L"Global\\RC4_RequestSemaphore";