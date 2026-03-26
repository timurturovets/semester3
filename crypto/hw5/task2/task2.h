
#pragma once

#include <cstdint>

namespace tasks {
    class probabilistic_primality_test {
    public:
        struct result {
            bool is_probably_prime;
            double achieved_probability;
            int iterations;
        };

        virtual ~probabilistic_primality_test() = default;
        virtual result test(std::int64_t value, double min_probability) = 0;
    };

    class abstract_probabilistic_primality_test : public probabilistic_primality_test {
    public:
        result test(std::int64_t value, double min_probability) override;

    protected:
        virtual bool run_iteration(std::int64_t value, std::int64_t witness) = 0;
        virtual std::int64_t choose_witness(std::int64_t value);
        [[nodiscard]] virtual double composite_witness_probability() const = 0;
    };

    class fermat_primality_test final : public abstract_probabilistic_primality_test {
    protected:
        bool run_iteration(std::int64_t value, std::int64_t witness) override;
        [[nodiscard]] double composite_witness_probability() const override;
    };
}
