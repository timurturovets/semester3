cmake_minimum_required(VERSION 4.1)
project(hw5)

set(CMAKE_CXX_STANDARD 20)

add_executable(hw5 main.cpp
        menu.cpp
        task1/run.h
        task1/number_theory_service.h
        task1/number_theory_service.cpp
        task1/task1.h
        task1/task1.cpp
        task2/run.h
        task2/task2.cpp
        task2/task2.h)
