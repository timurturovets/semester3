#pragma once

#include <cstddef>
#include <cstdint>
#include <filesystem>
#include <future>
#include <string>
#include <vector>

namespace tasks {
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

    struct cipher_config {
        algorithm_kind algorithm{};
        cipher_mode mode{};
        padding_mode padding{};
        std::vector<std::uint8_t> key;
        std::vector<std::uint8_t> iv;
        std::size_t thread_count{1U};
    };

    struct encrypted_buffer {
        std::vector<std::uint8_t> bytes;
        std::uint64_t original_size{};
    };

    class thread_pool {
    public:
        explicit thread_pool(std::size_t thread_count);
        ~thread_pool();

        thread_pool(const thread_pool&) = delete;
        thread_pool& operator=(const thread_pool&) = delete;
        thread_pool(thread_pool&&) = delete;
        thread_pool& operator=(thread_pool&&) = delete;

        void enqueue(std::function<void()> task);
        std::size_t size() const;
        void close();

    private:
        struct impl;
        std::unique_ptr<impl> impl_;
    };

    encrypted_buffer encrypt_bytes_sync(const std::vector<std::uint8_t>& plain, const cipher_config& config);
    std::vector<std::uint8_t> decrypt_bytes_sync(const encrypted_buffer& encrypted, const cipher_config& config);

    std::future<std::filesystem::path> encrypt_file_async(
        thread_pool& pool,
        const std::filesystem::path& input_path,
        const std::filesystem::path& output_path,
        const cipher_config& config
    );

    std::future<std::filesystem::path> decrypt_file_async(thread_pool& pool,
        const std::filesystem::path& input_path,
        const std::filesystem::path& output_path,
        const cipher_config& config
    );

    std::string to_string(algorithm_kind kind);
    std::string to_string(cipher_mode mode);
    std::string to_string(padding_mode padding);

    void run();
}