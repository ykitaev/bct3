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

vector<string> ix = { initializer_list<string>{"a", "bb", "cc", "dddddd"} };
int main()
{
	 auto i = PowerIter();
	 PowerIter p;
	 for (p= begin(i); p != i.end(); ++p)
	 {
	 	//cout << (*p).c_str() << endl;
	 }
	 cout << get<0>(*p) << "^" << get<1>(*p) << endl;
	 BloomFilter<1, 3> bb(12, 0.2, nullptr);
	return 0;
}


