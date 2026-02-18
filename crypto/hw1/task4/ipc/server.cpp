#include <windows.h>
#include <iostream>
#include <vector>

#include "../auxiliary/shared_memory.h"
#include "../rc4/rc4.h"

#include "ipc.h"

namespace tasks {

    class server {
    public:
        static void run() {
            size_t shmSize = sizeof(SharedMemory);
            HANDLE hMap = CreateFileMappingW(INVALID_HANDLE_VALUE,
                nullptr,
                PAGE_READWRITE,
                0,
                shmSize,
                SHM_NAME
                );

            if (!hMap) {
                std::cerr << "Failed to create file mapping";
                return;
            }

            void *view = MapViewOfFile(
                hMap,
                FILE_MAP_ALL_ACCESS,
                0, 0, 0
            );

            if (!view) {
                std::cerr << "Failed to create map view of file";
                return;
            }

            auto *shared = reinterpret_cast<SharedMemory *>(view);

            ZeroMemory(shared, shmSize);

            HANDLE slot_sem = CreateSemaphoreW(
                nullptr,
                MAX_SESSIONS,
                MAX_SESSIONS,
                SLOT_SEM_NAME
                );

            if (!slot_sem) {
                std::cerr << "Failed to create slot semaphore";
                return;
            }

            HANDLE req_sem = CreateSemaphoreW(
                nullptr,
                0,
                MAX_SESSIONS,
                REQ_SEM_NAME
            );

            if (!req_sem) {
                std::cerr << "Failed to create request semaphore";
                return;
            }

            std::cout << "Server listening";

            // ReSharper disable once CppDFAEndlessLoop
            while (true) {
                WaitForSingleObject(req_sem, INFINITE);

                for (size_t i = 0; i < MAX_SESSIONS; i++) {
                    SessionSlot &slot = shared->slots[i];

                    if (slot.busy == 1 && slot.status == 1) {

                        // TODO worker thread

                        std::vector<uint8_t> key(slot.key, slot.key + slot.key_size);

                        std::vector data(slot.data, slot.data + slot.data_size);

                        try {
                            RC4 rc4(key);
                            rc4.process(data);

                            memcpy(slot.data, data.data(), slot.data_size);
                            slot.status = 2;
                        } catch (...) {
                            slot.status = 0;
                            slot.busy = 0;
                        }
                    }
                }
            }

            UnmapViewOfFile(view);
            CloseHandle(hMap);
            CloseHandle(slot_sem);
            CloseHandle(req_sem);
        }
    };
}