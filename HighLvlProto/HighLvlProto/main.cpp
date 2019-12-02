#include <iostream>
#include <vector>
#include <sstream>
#include <iomanip>
#include <algorithm>


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

unsigned long long l = 0;							// message length in bits
std::vector<unsigned char> bytes;					// vector of bytes containing input message
std::vector<std::vector<uint32_t>> M;				// vector of vectors of bytes containing splitted input message
std::vector<std::vector<uint32_t>> H;				// vector of vectors of hash values (hashed message)
uint32_t W[64];										// message schedule array
int N;												// number of blocks in padded message
static const char* dict = "0123456789abcdef";		// dictionary for translating string to hex


void string_to_bytes(const std::string &str)
{
	// converting input string to hex string
	std::string hex_str;
	unsigned char c;
	for (int i = 0; i < str.length(); i++)
	{
		c = str[i];
		hex_str.push_back(dict[c >> 4]);
		hex_str.push_back(dict[c & 15]);
	}

	// converting hex string to bytes
	for (int i = 0, j = 0; i < hex_str.length() / 2; ++i, j += 2)
	{
		unsigned char byte = std::stoi(hex_str.substr(j, 2), nullptr, 16);
		bytes.push_back(byte);
		l += 8;
	}
}

// calculating number of zeroes to be appended to message during padding
int calculate_k()
{
	int k = 0;
	while ((k + 1 + l) % 512 != 448) 
		k++;
	return k;
}

// prepare message for hashing
void pad_msg()
{
	int k = calculate_k();

	// append single one and 7 zeroes as 128 byte (1000 0000)
	bytes.push_back(0x80);
	k -= 7;

	// append zeroes rest of zeroes as bytes (0000 0000)
	for (int i = 0; i < k / 8; i++)
		bytes.push_back(0);

	// append 64 bit equal to message length (l as 64-bit)
	for (int i = 0; i < 9; i++)
		bytes.push_back(l >> (64 - i * 8));
}

// split message into N 512-bit blocks and saves N
void split_msg()
{
	uint32_t n = 0;
	for (int i = 0; n < bytes.size() / 64; n++)
	{
		// saving blocks as sixteen 32-bit words
		std::vector<uint32_t> block(16);
		for (int j = 0; j < 16; j++)
		{
			uint32_t word = 0;
			for (int k = 0; k < 4; k++, i++) 
			{
				word <<= 8;
				word |= bytes[i];
			}

			// saving word to block
			block[j] = word;
		}

		// saving message block
		M.push_back(block);
	}

	// saving number of blocks
	N = n;
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

// function which saves initial hash values
void init_hash_vals()
{
	std::vector<uint32_t> h = { 0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19 };
	H.push_back(h);
}

// hashing function
void calculate_hash()
{
	std::vector<uint32_t> hi(8);
	uint32_t T1, T2;						// temporary words
	uint32_t a, b, c, d, e, f, g, h;		// hash values of current iteration
	for (int i = 1; i <= N; i++)
	{
		// preparing message schedule
		for (int t = 0; t <= 15; t++)
			W[t] = M[i - 1][t];
		for (int t = 16; t <= 63; t++)
			W[t] = small_sigma1(W[t - 2]) + W[t - 7] + small_sigma0(W[t - 15]) + W[t - 16];

		// getting hash values of previous iteration
		a = H[i - 1][0];
		b = H[i - 1][1];
		c = H[i - 1][2];
		d = H[i - 1][3];
		e = H[i - 1][4];
		f = H[i - 1][5];
		g = H[i - 1][6];
		h = H[i - 1][7];

		// calculating new hash values
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

		// saving hash values of current iteration
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

// function used for debugging string to hex translation by translating hex to string
void output_hash_debug()
{
	// saving current hex output to string
	std::ostringstream hex_os;
	for (int i = 0; i < 8; ++i)
		hex_os << std::hex << std::setw(8) << std::setfill('0') << H[N][i];
	std::string hex_out = hex_os.str();
	std::string output;
	if (hex_out.length() & 1)
		std::cout << "odd output\n";

	// hex -> string translation
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
	
	// printing translated string (if weird illegal signs are displayed - something is wrong)
	std::cout << "\n\nOutput: " << output << std::endl << "Output orig: " << hex_out << std::endl;
}

// function for printing hashed output message
void output_hash()
{
	for (int i = 0; i < 8; ++i)
		std::cout << std::hex << std::setw(8) << std::setfill('0') << H[N][i];
	std::cout << std::endl;
}

// main function
int main()
{
	// some custom message
	//const std::string input = "The quick brown fox jumps over the lazy dog";

	// control string. For empty string this is expected output:
	// e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
	const std::string input = "";
	
	// preparing message
	string_to_bytes(input);
	pad_msg();
	split_msg();

	// hashing
	init_hash_vals();
	calculate_hash();

	// returning hashed value
	output_hash();
	
	return 0;
}