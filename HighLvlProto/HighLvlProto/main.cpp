#include <iostream>
#include <vector>
#include <sstream>
#include <iomanip>
#include <algorithm>


// initial array of hash constants
const unsigned int K[] = {
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
static const char* dict = "0123456789abcdef";
unsigned int a, b, c, d, e, f, g, h;

void string_to_hash(const std::string &str)
{
	std::string hex_str;
	unsigned char c;
	for (int i = 0; i < str.length(); i++)
	{
		c = str[i];
		hex_str.push_back(dict[c >> 4]);
		hex_str.push_back(dict[c & 15]);
	}
	for (int i = 0, j = 0; i < hex_str.length() / 2; ++i, j += 2)
	{
		unsigned char byte = std::stoi(hex_str.substr(j, 2), nullptr, 16);
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

void calculate_hash()
{
	unsigned int T1, T2;
	std::vector<unsigned int> hi(8);
	for (int i = 1; i <= N; i++)
	{
		for (int t = 0; t <= 15; t++)
			W[t] = M[i - 1][t];
		for (int t = 16; t <= 63; t++)
			W[t] = small_sigma1(W[t - 2]) + W[t - 7] + small_sigma0(W[t - 15]) + W[t - 16];

		a = H[i - 1][0];
		b = H[i - 1][1];
		c = H[i - 1][2];
		d = H[i - 1][3];
		e = H[i - 1][4];
		f = H[i - 1][5];
		g = H[i - 1][6];
		h = H[i - 1][7];

		for (int t = 0; t <= 63; t++)
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

		hi[0] = a + H[i - 1][0];
		hi[1] = b + H[i - 1][1];
		hi[2] = c + H[i - 1][2];
		hi[3] = d + H[i - 1][3];
		hi[4] = e + H[i - 1][4];
		hi[5] = f + H[i - 1][5];
		hi[6] = g + H[i - 1][6];
		hi[7] = h + H[i - 1][7];
		H.push_back(hi);
	}
}

void output_hash_debug()
{
	std::ostringstream hex_os;
	for (int i = 0; i < 8; ++i)
		hex_os << std::hex << std::setw(8) << std::setfill('0') << H[N][i];
	std::string hex_out = hex_os.str();
	std::string output;
	if (hex_out.length() & 1)
		std::cout << "odd output\n";

	char a, b;
	for (int i = 0; i < hex_out.length(); i += 2)
	{
		a = hex_out[i];
		const char* p = std::lower_bound(dict, dict + 16, a);
		if (*p != a)
			std::cout << "not a hex A ";

		b = hex_out[i + 1];
		const char* q = std::lower_bound(dict, dict + 16, b);
		if (*q != b)
			std::cout << "not a hex B ";

		output.push_back(((p - dict) << 4) | (q - dict));
	}
	std::cout << "\n\nOutput: " << output << std::endl << "Output orig: " << hex_out << std::endl;
}

void output_hash()
{
	for (int i = 0; i < 8; ++i)
		std::cout << std::hex << std::setw(8) << std::setfill('0') << H[N][i];
	std::cout << std::endl;
}

int main()
{
	//const std::string input = "The quick brown fox jumps over the lazy dog";
	const std::string input = "";
	string_to_hash(input);
	pad_msg();
	split_msg();
	init_hash_vals();
	calculate_hash();
	output_hash();
	return 0;
}