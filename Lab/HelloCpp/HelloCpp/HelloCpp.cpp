// HelloCpp.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>
#include <cstdint>
#include <vector>
#include <sstream>
#include <string>
#include <tuple>

#include "bigint-2010.04.30\BigIntegerLibrary.hh"
#include "BloomFilterBigInteger.cpp"

using namespace std; 

class PowerIter
{
private: 
	int pow;
	int base;
	int k;
public:
	PowerIter() : base{ 3 }, pow{ 3 }, k{ 2 }
	{
	}

	PowerIter(int b, int p) : base{ b }, pow{ p }, k{ 2 }
	{
	}

	PowerIter end() const 
	{
		return PowerIter(889284, 3);
	}

	PowerIter begin() const 
	{ 
		return PowerIter(2, 3); 
	}

	PowerIter& operator++() {
		if (base >= 3 && pow <= 201)
		{
			++pow;
			--base;
			return *this;
		}
		else
		{
			++k;
			base = k;
			pow = 3;
			return *this;
		}
	}
	PowerIter operator++(int) { 
		auto temp = *this; 
		if (base >= 3 && pow <= 201)
		{
			++pow;
			--base;
		}
		else
		{
			++k;
			base = k;
			pow = 3;
		}
		return temp; 
	}
	tuple<int,int> operator*() {
		return make_tuple(base, pow);
	}
	bool operator==(PowerIter x) { return x.base == base && x.pow == pow ; }
	bool operator!=(PowerIter x) { return x.base != base || x.pow != pow; }
};


BigInteger xpow(const int b, const int e)
{
	if (e == 1)
		return b;
	else
	{
		auto e1 = e / 2;
		auto e2 = e - e1;
		auto ax1 = xpow(b, e1);
		auto ax2 = xpow(b, e2);
		return ax1 * ax2;
	}
}
unsigned int crc32_internal(unsigned int crc, const char *buf, size_t size);
int crc(BigInteger big)
{
	// Achievement unlocked: using reinterpret_cast!
	return crc32_internal(0xBABABABA, reinterpret_cast<const char *>(big.mag.blk), big.mag.getLength() * sizeof(unsigned long));
}

vector<string> ix = { initializer_list<string>{"a", "bb", "cc", "dddddd"} };
int main()
{
	 BloomFilter<8> bf(178000000, 0.004f, crc);

	 auto i = PowerIter();
	 PowerIter p;
	 auto cnt = 0;
	 long checksum = 0;
	 for (p= begin(i); p != i.end(); ++p)
	 {
		 auto base = get<0>(*p);
		 auto exp = get<1>(*p);
		 auto ax = xpow(base, exp);
		 auto c = crc(ax);
		 auto hc = ax.GetHashCode();
		 checksum += (c + hc);
		 //bf.Add(ax);
		 ++cnt;
		 if (cnt % 1000000 == 0)
			 cout << cnt << endl;
	 }
	 // cout << get<0>(*p) << "^" << get<1>(*p) << endl;

	 // cout << bf.Truthiness();
	 cout << checksum;
	return 0;
}


