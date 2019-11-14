#include <iostream>
#include <vector>
#include <string>


// initial array of hash constants
const unsigned int k[] = {
	0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
	0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
	0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
	0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
	0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
	0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
	0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
	0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
};

unsigned long long l = 0;		// message length in bits
std::vector<unsigned char> bytes;
std::vector<std::vector<unsigned int>> M;
std::vector<std::vector<unsigned int>> H;
unsigned int W[64];
int N;
unsigned int a, b, c, d, e, f, g, h;

void string_to_hash(std::string str)
{
	for (int i = 0, j = 0; i < str.length(); i++, j += 2)
	{
		unsigned char byte = std::stoi(str.substr(j, 2), nullptr, 16);
		bytes.push_back(byte);
		l += 8;
	}
}

int calculate_k()
{
	int k = 0;
	while ((k + 1 + l) % 512 != 448) k++;
	return k;
}

void pad_msg()
{
	int k = calculate_k();

	bytes.push_back(0x80);
	k -= 7;

	for (int i = 0; i < k / 8; i++)
		bytes.push_back(0);

	for (int i = 0; i < 9; i++)
		bytes.push_back(l >> (64 - i * 8));
}

void split_msg()
{
	unsigned int n = 0;
	for (int i = 0; n < bytes.size() / 64; n++)
	{
		std::vector<unsigned int> block(16);
		for (int j = 0; j < 16; j++)
		{
			unsigned int word = 0;
			for (int k = 0; k < 4; k++, i++) 
			{
				word <<= 8;
				word |= bytes[i];
			}
			block[j] = word;
		}
		M.push_back(block);
	}
	N = n;
}

unsigned int rotate_right(unsigned int input, unsigned int bit_count)
{
	return (input >> bit_count) | (input << (32 - bit_count));
}

unsigned int Ch(unsigned int x, unsigned int y, unsigned int z)
{
	return ((x & y) ^ (~x & z));
}

unsigned int Maj(unsigned int x, unsigned int y, unsigned int z)
{
	return ((x & y) ^ (x & z) ^ (y & z));
}

unsigned int big_sigma0(unsigned int x)
{
	return (rotate_right(x, 2) ^ rotate_right(x, 13) ^ rotate_right(x, 22));
}

unsigned int big_sigma1(unsigned int x)
{
	return (rotate_right(x, 6) ^ rotate_right(x, 11) ^ rotate_right(x, 25));
}

unsigned int small_sigma0(unsigned int x)
{
	return (rotate_right(x, 7) ^ rotate_right(x, 18) ^ (x >> 3));
}

unsigned int small_sigma1(unsigned int x)
{
	return (rotate_right(x, 17) ^ rotate_right(x, 19) ^ (x >> 10));
}

void init_hash_vals()
{
	std::vector<unsigned int> h = { 0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19 };
	H.push_back(h);
}

int main()
{
	std::cout << "Hello world!";
	return 0;
}