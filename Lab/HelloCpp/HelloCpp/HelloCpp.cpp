// HelloCpp.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>
#include <cstdint>
#include <vector>
#include <sstream>
#include <string>
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
	string operator*() {
		std::ostringstream stringStream;
		stringStream << base << "^" << pow;
		std::string copyOfStr = stringStream.str();
		return copyOfStr;
	}
	bool operator==(PowerIter x) { return x.base == base && x.pow == pow ; }
	bool operator!=(PowerIter x) { return x.base != base || x.pow != pow; }
};




vector<string> ix = { initializer_list<string>{"a", "bb", "cc", "dddddd"} };
int main()
{
	static_assert(sizeof(int) >= 4, "Ints should be at least 4 bytes lol");

	 auto i = PowerIter();
	 PowerIter p;
	 for (p= begin(i); p != i.end(); ++p)
	 {
	 	//cout << (*p).c_str() << endl;
	 }
	 cout << (*p).c_str() << endl;
	return 0;
}

