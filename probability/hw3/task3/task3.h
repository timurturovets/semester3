#pragma once

#include <random>
#include <vector>
#include <string>

namespace tasks {
    class task3_aux {
    public:
        struct Args {
            int assembly_machines = 0;
            int recycle_machines = 0;
            int n = 0;
            int t_assembly = 0;
            int t_recycle = 0;
            int am_modules = 0;
            int rm_modules = 0;
            bool has_seed = false;
            unsigned int seed = 0;
            bool has_max_time = false;
            int max_time = 0;
            std::vector<double> recipe;
            std::vector<double> supply;
        };

        struct Job {
            bool busy = false;
            int finish_time = 0;
            int base_quality = 1;
        };
    private:
        bool parse_int(const std::string &text, int &value);

        bool parse_double(const std::string &text, double &value);

        bool parse_args(int argc, char **argv, Args &args, std::string &error);

        int effective_time(int base, int modules);

        int sample_quality(int base_quality, int modules, std::mt19937 &rng);

        int choose_component_quality(const std::vector<std::vector<double>> &components,
                                     const std::vector<double> &recipe);

        int choose_recycle_quality(const std::vector<double> &products);

        void print_times(const std::string &label, const std::vector<int> &times);
    };
}