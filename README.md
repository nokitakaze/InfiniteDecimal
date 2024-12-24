# Infinite Decimal

[![Build status](https://ci.appveyor.com/api/projects/status/jpc7733fv1dv6ioe/branch/master?svg=true)](https://ci.appveyor.com/project/nokitakaze/infinitedecimal/branch/master)
[![Test status](https://img.shields.io/appveyor/tests/nokitakaze/infinitedecimal/master)](https://ci.appveyor.com/project/nokitakaze/infinitedecimal/branch/master)
![Test status](https://github.com/nokitakaze/InfiniteDecimal/actions/workflows/ci.yml/badge.svg?branch=master)
![Test status](https://github.com/nokitakaze/InfiniteDecimal/actions/workflows/ci-windows.yml/badge.svg?branch=master)
[![codecov](https://codecov.io/gh/nokitakaze/infinitedecimal/branch/master/graph/badge.svg)](https://codecov.io/gh/nokitakaze/infinitedecimal)
[![Nuget version](https://badgen.net/nuget/v/InfiniteDecimal)](https://www.nuget.org/packages/InfiniteDecimal)
[![Total nuget downloads](https://badgen.net/nuget/dt/InfiniteDecimal)](https://www.nuget.org/packages/InfiniteDecimal)

This repository provides a powerful tool for numerical computation and analysis. It offers a decimal data type with
potentially infinite precision, enabling complex mathematical operations where high accuracy is required without any
limitation on the number of digits after the decimal point. The flexibility and versatility of this repository make it
an essential tool for anyone who deals with high-level numerical computations.

It represents a number as a * 10^-b, where a is a BigInteger mantissa, and b is an int greater than or equal to 0.

## Examples

Initializing variables and construction

```csharp
var precision = 5;
var a = new BigDec(1, precision);
var b = new BigDec(1m, precision + 1);
var c = new BigDec(0.3d, precision + 2);
var d = new BigDec(0.3);
var f = a.WithPrecision(1337);
var g = new BigDec(BigInteger.One);

var h = new BigDec(1000, 3, 500); // 1 because 1000 * 10^-3 = 1
var i = new BigDec(1000m, 4, 500); // 0.1 because 1000 * 10^-4 = 0.1
...
```

Deconstruction

```csharp
var b = new BigDec(1001, 3, 500);
Console.WriteLine(
    "{0} {1}\n{2} {3}",
    b.Mantissa.GetType(),
    b.Mantissa,
    b.Offset.GetType(),
    b.Offset
);
// System.Numerics.BigInteger 1001
// System.Int32 3

var a = new BigDec(1000, 3, 500);
Console.WriteLine(
    "{0} {1}\n{2} {3}",
    a.Mantissa.GetType(),
    a.Mantissa,
    a.Offset.GetType(),
    a.Offset
);
// System.Numerics.BigInteger 1
// System.Int32 0
```

Primitive arithmetic operations

```csharp
var g = a + b;
var h = a * new BigDec(156);
var i = new BigDec(148, 1000) / g;
var j = a - b;

var k1 = new BigDec(10, 2) / 3; // 3.333333333333333333, because 3 parsed with default precision = 18
var k2 = new BigDec(10, 2) / new BigDec(3, 2); // 3.33
```

Power and Sqrt

```csharp
var a = new BigDec(12.34);
var b = a.Sqrt();
Console.WriteLine("{0}", b); // 3.51283361405005916
var c = b * b;
Console.WriteLine("{0}", c - a); // -0.000000000000000005
```

Ln & Exp

```csharp
var a = new BigDec(12.34);
var b = a.Pow(15.58m);
Console.WriteLine("{0}", a); // 12.34
Console.WriteLine("{0}", b); // 100621361186662831.979620536026973505
var c = a.Ln();
var d = c.Exp();
Console.WriteLine("{0}", a - d); // 0.000000000000000005
```

Parsing

```csharp
var a = BigDec.Parse("-12.34");
Console.WriteLine("{0}", a); // -12.34
var b = BigDec.Parse("13.e+1");
var c = BigDec.Parse("13.37e+1");
Console.WriteLine("{0}", b); // 130
Console.WriteLine("{0}", c); // 133.7
```

Comparision

```csharp
Console.WriteLine("{0}", new BigDec(12.34) / 2 == new BigDec(6.17m)); // True
Console.WriteLine("{0}", new BigDec(0.1) + new BigDec(0.2) == new BigDec(0.3)); // True
Console.WriteLine("{0}", new BigDec(0.1m) + new BigDec(0.2m) == new BigDec(0.3m)); // True
Console.WriteLine("{0}", new BigDec(0.1m) <= new BigDec(0.2m)); // True
Console.WriteLine("{0}", new BigDec(0.1m) > new BigDec(0.2m)); // False
```

Included constants

```csharp
var diff = (BigDec.E - Math.E).Abs();
Console.WriteLine("{0}", diff <= 0.000_000_001m); // True
```

```csharp
Console.WriteLine("{0}", BigDec.E);
Console.WriteLine("{0}", BigDec.PI);
Console.WriteLine("{0}", (new BigDec(1, 1000).Exp() - BigDec.E).Abs()); // 0.000...0002 (999 zeroes)
Console.WriteLine("{0}", new BigDec(1, 10000).ExpWithBigPrecision()); // can correctly calculate Euler's number to the demanded 10000 digits
```

```csharp
Console.WriteLine("{0}", new BigDec(0).Pow(0)); // 1
var epsilon = BigDec.PowFractionOfTen(3); // 0.001
BigInteger a = BigDec.Pow10BigInt(5); // 100000
```

## License

Licensed under the Apache License.

This software is provided **"AS IS" WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied**.
