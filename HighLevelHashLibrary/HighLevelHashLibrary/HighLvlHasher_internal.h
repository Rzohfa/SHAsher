#ifndef HIGHLVLHASHER_INTERNAL
#define HIGHLVLHASHER_INTERNAL

uint32_t rotate_right (uint32_t input, uint32_t bit_count);
uint32_t Ch (uint32_t x, uint32_t y, uint32_t z);
uint32_t Maj (uint32_t x, uint32_t y, uint32_t z);
uint32_t big_sigma0 (uint32_t x);
uint32_t big_sigma1 (uint32_t x);
uint32_t small_sigma0 (uint32_t x);
uint32_t small_sigma1 (uint32_t x);
void init_hash_vals();
void calculate_hash();
void output_hash();

#endif // !HIGHLVLHASHER_INTERNAL