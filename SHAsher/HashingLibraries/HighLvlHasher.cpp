#include "HighLvlHasher.h"
#include <iostream>
#include <iomanip>
#include <sstream>
#include <vector>
#include <cstdint>

///////////////////////////////////////////////////////////
/*
 * Autor: Dominik Wolny
 * Informatyka
 * Semestr: 5
 * Grupa dziekanska: 1-2
 *
 * Temat: Hashowanie algorytmem SHA-256
 */
 ///////////////////////////////////////////////////////////

namespace hasher
{
	// array of hash constants
	const uint32_t K[] = {
		0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
		0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
		0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
		0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
		0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
		0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
		0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
		0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
	};


	// Exported hashing function
	void _cdecl hashCpp(uint32_t* bytes, int Nblocks, char* return_buffer)
	{
		uint32_t* M;				// vector of vectors of bytes containing splitted input message
		uint32_t H[8];				// vector of vectors of hash values (hashed message)
		uint32_t W[64];				// message schedule array
		uint32_t N;					// number of blocks in padded message

		// saving arguments to local variables
		M = bytes;
		N = Nblocks;

		// Preparing initial hash values according to point 5.3.3 of specification
		H[0] = 0x6a09e667;
		H[1] = 0xbb67ae85;
		H[2] = 0x3c6ef372;
		H[3] = 0xa54ff53a;
		H[4] = 0x510e527f;
		H[5] = 0x9b05688c;
		H[6] = 0x1f83d9ab;
		H[7] = 0x5be0cd19;

		uint32_t T1, T2;						// temporary words
		uint32_t a, b, c, d, e, f, g, h;		// hash values of current iteration
		
		// main hash loop
		for (unsigned int i = 0; i < N; ++i)
		{
			// preparing message schedule
			for (int t = 0; t < 16; t++)
				W[t] = M[(i * 16) + t];
			for (int t = 16; t < 64; t++)
				W[t] = small_sigma1(W[t - 2]) + W[t - 7] + small_sigma0(W[t - 15]) + W[t - 16];

			// getting hash values of previous iteration
			a = H[0];
			b = H[1];
			c = H[2];
			d = H[3];
			e = H[4];
			f = H[5];
			g = H[6];
			h = H[7];

			// calculating new hash values 
			for (int t = 0; t < 64; t++)
			{
				T1 = h + big_sigma1(e) + Ch(e, f, g) + K[t] + W[t];
				T2 = big_sigma0(a) + Maj(a, b, c);
				h = g;
				g = f;
				f = e;
				e = d + T1;
				d = c;
				c = b;
				b = a;
				a = T1 + T2;
			}

			// saving hash values of current iteration
			H[0] += a;
			H[1] += b;
			H[2] += c;
			H[3] += d;
			H[4] += e;
			H[5] += f;
			H[6] += g;
			H[7] += h;
		}

		// prepare output for sending to main app
		std::ostringstream hex_os;
		for (int i = 0; i < 8; ++i)
			hex_os << std::hex << std::setw(8) << std::setfill('0') << H[i];

		// send output to main app
		strcpy_s(return_buffer, 65, hex_os.str().c_str());
	}

	// function performing bitwise right rotation (circular right shift)
	uint32_t rotate_right(uint32_t input, uint32_t bit_count)
	{
		return (input >> bit_count) | (input << (32 - bit_count));
	}

	// function based on 4.2 formula from specification
	uint32_t Ch(uint32_t x, uint32_t y, uint32_t z)
	{
		return ((x & y) ^ (~x & z));
	}

	// function based on 4.3 formula from specification
	uint32_t Maj(uint32_t x, uint32_t y, uint32_t z)
	{
		return ((x & y) ^ (x & z) ^ (y & z));
	}

	// function based on 4.4 formula from specification
	uint32_t big_sigma0(uint32_t x)
	{
		return (rotate_right(x, 2) ^ rotate_right(x, 13) ^ rotate_right(x, 22));
	}

	// function based on 4.5 formula from specification
	uint32_t big_sigma1(uint32_t x)
	{
		return (rotate_right(x, 6) ^ rotate_right(x, 11) ^ rotate_right(x, 25));
	}

	// function based on 4.6 formula from specification
	uint32_t small_sigma0(uint32_t x)
	{
		return (rotate_right(x, 7) ^ rotate_right(x, 18) ^ (x >> 3));
	}

	// function based on 4.7 formula from specification
	uint32_t small_sigma1(uint32_t x)
	{
		return (rotate_right(x, 17) ^ rotate_right(x, 19) ^ (x >> 10));
	}
}
