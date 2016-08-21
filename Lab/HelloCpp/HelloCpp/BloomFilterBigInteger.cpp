#include <bitset>
#include "bigint-2010.04.30\BigInteger.hh"
#include<vector>
using namespace std;

template <int K>
class BloomFilter
{
	int _hashFunctionCount;
	//bitset<2045611135> *_hashBits;
	vector<bool>* _hashBits;
	typedef int(*hash)(BigInteger b);
	hash _getHashSecondary;

public:
	/// <summary>
	/// Creates a new Bloom filter.
	/// </summary>
	/// <param name="capacity">The anticipated number of items to be added to the filter. More than this number of items can be added, but the error rate will exceed what is expected.</param>
	/// <param name="errorRate">The accepable false-positive rate (e.g., 0.01F = 1%)</param>
	/// <param name="hashFunction">The function to hash the input values. Do not use GetHashCode(). If it is null, and T is string or int a hash function will be provided for you.</param>
	/// <param name="m">The number of elements in the BitArray.</param>
	/// <param name="k">The number of hash functions to use.</param>
	BloomFilter(int capacity, float errorRate, hash hashFunction)
	{
		// validate the params are in range
		if (capacity < 1)
		{
			throw "capacity must be > 0";
		}

		if (errorRate >= 1 || errorRate <= 0)
		{
			throw "errorRate must be between 0 and 1, exclusive.";
		}

		// set the secondary hash function
		if (hashFunction == nullptr)
		{
			throw "no hash function provided";
		}
		else
		{
			_getHashSecondary = hashFunction;
		}

		_hashFunctionCount = K;
		_hashBits = new vector<bool>(2045611135);
	}

	/// <summary>
	/// The ratio of false to true bits in the filter. E.g., 1 true bit in a 10 bit filter means a truthiness of 0.1.
	/// </summary>
	double Truthiness()
	{
		return (double)TrueBits() / _hashBits->count();
	}

	// Haha I remember to do this! Unique pointer would have been neater but this is cooler.
	~BloomFilter()
	{
		if (_hashBits != nullptr) delete _hashBits;
	}

	/// <summary>
	/// Adds a new item to the filter. It cannot be removed.
	/// </summary>
	/// <param name="item">The item.</param>
	void Add(BigInteger item)
	{
		// start flipping bits for each hash of item
		int primaryHash = item.GetHashCode();
		int secondaryHash = _getHashSecondary(item);
		for (auto i = 0; i < _hashFunctionCount; ++i)
		{
			int hash = ComputeHash(primaryHash, secondaryHash, i);
			(*_hashBits)[hash] = true;
		}
	}

	/// <summary>
	/// Checks for the existance of the item in the filter for a given probability.
	/// </summary>
	/// <param name="item"> The item. </param>
	/// <returns> The <see cref="bool"/>. </returns>
	bool Contains(BigInteger item)
	{
		int primaryHash = item.GetHashCode();
		int secondaryHash = _getHashSecondary(item);
		for (int i = 0; i < _hashFunctionCount; ++i)
		{
			int hash = ComputeHash(primaryHash, secondaryHash, i);
			if (_hashBits[hash] == false)
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// The true bits.
	/// </summary>
	/// <returns> The <see cref="int"/>. </returns>
	int TrueBits()
	{
		int output = 0;
		for (auto i = 0; i < _hashBits->count; ++i)
		{
			if (_hashBits[i] == 1)
			{
				++output;
			}
		}
		return output;
	}

	/// <summary>
	/// Performs Dillinger and Manolios double hashing. 
	/// </summary>
	/// <param name="primaryHash"> The primary hash. </param>
	/// <param name="secondaryHash"> The secondary hash. </param>
	/// <param name="i"> The i. </param>
	/// <returns> The <see cref="int"/>. </returns>
	int ComputeHash(int primaryHash, int secondaryHash, int i)
	{
		int resultingHash = (primaryHash + (i * secondaryHash)) % 2045611135;
		return abs(resultingHash);
	}
};