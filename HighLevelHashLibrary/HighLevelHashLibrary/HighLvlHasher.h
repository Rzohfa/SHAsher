#pragma once

#ifdef HIGHLVLHASHER_EXPORTS
#define HIGHLVLHASHER_API __declspec(dllexport)
#else
#define HIGHLVLHASHER_API __declspec(dllimport)
#endif

extern "C" HIGHLVLHASHER_API void hash();