cmake_minimum_required(VERSION 4.1)
project(hw3)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTOUIC ON)
set(CMAKE_AUTORCC ON)

find_package(Qt6 COMPONENTS Widgets QUIET)
if (Qt6_FOUND)
    set(QT_WIDGETS_LIB Qt6::Widgets)
else()
    find_package(Qt5 COMPONENTS Widgets REQUIRED)
    set(QT_WIDGETS_LIB Qt5::Widgets)
endif()

add_executable(hw3 main.cpp
        menu.cpp
        task1/run.h
        task1/task1.cpp
        task1/task1.h
        task2/run.h
        task2/task2.cpp
        task3/task3.h
        task3/run.h
        task3/task3.cpp
)

target_link_libraries(hw3 PRIVATE ${QT_WIDGETS_LIB})
