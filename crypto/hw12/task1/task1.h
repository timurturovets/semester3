#ifndef HW12_TASK1_H
#define HW12_TASK1_H

#include <cstdint>
#include <random>
#include <string>
#include <vector>

namespace tasks {
    struct RabinKeyPair {
        uint64_t p;
        uint64_t q;
        uint64_t n;
    };

    class MillerRabin {
    public:
        static bool isProbablePrime(uint64_t n, int rounds, std::mt19937_64 &generator);
        static uint64_t generatePrime(unsigned int bitLength, int rounds, std::mt19937_64 &generator);
        static uint64_t modPow(uint64_t base, uint64_t exponent, uint64_t modulus);
        static uint64_t mulMod(uint64_t a, uint64_t b, uint64_t modulus);

    private:
        static uint64_t randomInRange(uint64_t left, uint64_t right, std::mt19937_64 &generator);
    };

    class RabinCipher {
    public:
        static RabinKeyPair generateKeyPair(unsigned int primeBitLength, int rounds, std::mt19937_64 &generator);
        static std::vector<uint64_t> encrypt(const std::vector<uint8_t> &data, uint64_t n);
        static std::vector<uint8_t> decrypt(const std::vector<uint64_t> &ciphertext, const RabinKeyPair &keyPair);

    private:
        static uint64_t modInverse(uint64_t a, uint64_t modulus);
        static std::vector<uint64_t> squareRootsModComposite(uint64_t value, const RabinKeyPair &keyPair);
        static uint64_t encodeByte(uint8_t value);
        static bool decodeByte(uint64_t value, uint8_t &decoded);
    };

    class ByteSequence {
    public:
        static std::vector<uint8_t> fromString(const std::string &text);
        static std::string toString(const std::vector<uint8_t> &bytes);
    };
}

#endif