/*Use this version if System.Numerics is not available.
//Activate it by putting a / at the beginning of the file
using System.Numerics;
using System.Runtime.Serialization;
using Newtonsoft.Json;

[System.Serializable]
public struct BigNumber {
    // Basically a BigInteger multiplied by a million to support fractions
    private const int DECIMAL_SCALE = 1000000;
    private const int MAGNITUDE_SCALE = 6;

    private static readonly string[] tiers = {
        "million", "billion", "trillion", "quadrillion", "quintillion",
        "sextillion", "septillion", "octillion", "nonillion", "decillion",
        "undecillion", "duodecillion", "tredecillion", "quattuordecillion", "quindecillion",
        "sexdecillion", "septendecillion", "octodecillion", "novemdecillion", "vigintillion"
    };

    private static readonly string[] tierShorthand = {
        "M", "B", "T", "Qa", "Qi",
        "Sx", "Sp", "O", "N", "D",
        "UD", "DD", "TD", "QaD", "QiD",
        "SxD", "SpD", "OD", "ND", "V"
    };

    [JsonProperty]
    [SerializeField]
    private double bigInteger;


    public BigNumber(int integerValue) {
        bigInteger = (double)(integerValue) * DECIMAL_SCALE;
    }

    public BigNumber(float floatValue) {
        bigInteger = (double)(floatValue * DECIMAL_SCALE);
    }

    public BigNumber(double doubleValue) {
        bigInteger = (double)(doubleValue * DECIMAL_SCALE);
    }

    // public BigNumber(double integerValue) {
    // 	bigInteger = integerValue * DECIMAL_SCALE;
    // }

    private BigNumber(double integerValue, bool unscaled = false) {
        if(unscaled)
            bigInteger = integerValue;
        else
            bigInteger = integerValue * DECIMAL_SCALE;
    }

    [JsonIgnore]
    public int Magnitude {
        get {
            if(Mathd.Abs(bigInteger) < DECIMAL_SCALE)
                return 0;
            return (Mathd.Abs(Mathd.Floor(bigInteger)).ToString().Length - 1 - MAGNITUDE_SCALE);
        }
    }

    [JsonIgnore]
    public string StringRepresentation {
        get {
            string representation = Mathd.Floor(bigInteger).ToString();
            int length = representation.Length;
            if(length > MAGNITUDE_SCALE)
                representation = representation.Insert(length - MAGNITUDE_SCALE, ".");
            else {
                if(length < MAGNITUDE_SCALE)
                    representation = new string('0', MAGNITUDE_SCALE - length) + representation;
                representation = "0." + representation;
            }
            return representation;
        }
    }

    public static implicit operator BigNumber(int number) {
        return new BigNumber(number);
    }

    public static implicit operator BigNumber(float number) {
        return new BigNumber(number);
    }

    public static implicit operator BigNumber(double number) {
        return new BigNumber(number);
    }

    public static implicit operator string(BigNumber number) {
        return number.ToString();
    }

    public static explicit operator BigNumber(string number) {
        string representation = System.String.Copy(number);
        int indexOfPeriod = representation.IndexOf('.');
        if(indexOfPeriod == -1)
            return new BigNumber(double.Parse(representation));
        int missingDigits = MAGNITUDE_SCALE + indexOfPeriod + 1 - representation.Length;
        if(missingDigits > 0)
            representation += new string('0', missingDigits);
        representation = representation.Substring(0, indexOfPeriod) + representation.Substring(indexOfPeriod + 1, MAGNITUDE_SCALE);
        return new BigNumber(double.Parse(representation), unscaled: true);
    }


    public static explicit operator float(BigNumber number) {
        return (float)number.bigInteger / (float)DECIMAL_SCALE;
    }

    public static bool operator ==(BigNumber left, BigNumber right) {
        return left.bigInteger == right.bigInteger;
    }

    public static bool operator !=(BigNumber left, BigNumber right) {
        return left.bigInteger != right.bigInteger;
    }

    public static BigNumber operator +(BigNumber left, BigNumber right) {
        double sum = left.bigInteger + right.bigInteger;
        return new BigNumber(sum, unscaled: true);
    }

    public static BigNumber operator ++(BigNumber num) {
        return num + 1;
    }

    public static BigNumber operator -(BigNumber number) {
        double newNumber = -number.bigInteger;
        return new BigNumber(newNumber, unscaled: true);
    }

    public static BigNumber operator -(BigNumber left, BigNumber right) {
        double sum = left.bigInteger - right.bigInteger;
        return new BigNumber(sum, unscaled: true);
    }

    public static BigNumber operator --(BigNumber num) {
        return num - 1;
    }

    public static BigNumber operator *(BigNumber left, BigNumber right) {
        double product = left.bigInteger * right.bigInteger;
        double multipliedDown = product / DECIMAL_SCALE;
        return new BigNumber(multipliedDown, unscaled: true);
    }

    public static BigNumber operator *(BigNumber bigNumber, double number) {
        double rightInteger = (double)number * DECIMAL_SCALE * DECIMAL_SCALE;
        double product = bigNumber.bigInteger * rightInteger;
        double multipliedDown = product / DECIMAL_SCALE / DECIMAL_SCALE;
        return new BigNumber(multipliedDown, unscaled: true);
    }

    public static BigNumber operator /(BigNumber left, BigNumber right) {
        double result = left.bigInteger * DECIMAL_SCALE / right.bigInteger;
        return new BigNumber(result, unscaled: true);
    }

    public static BigNumber operator /(BigNumber bigNumber, double number) {
        double rightInteger = number * DECIMAL_SCALE * DECIMAL_SCALE;
        double result = bigNumber.bigInteger * DECIMAL_SCALE * DECIMAL_SCALE / rightInteger;
        return new BigNumber(result, unscaled: true);
    }

    public static bool operator >(BigNumber left, BigNumber right) {
        return (left.bigInteger > right.bigInteger);
    }

    public static bool operator <(BigNumber left, BigNumber right) {
        return (left.bigInteger < right.bigInteger);
    }

    public static bool operator >=(BigNumber left, BigNumber right) {
        return (left.bigInteger >= right.bigInteger);
    }

    public static bool operator <=(BigNumber left, BigNumber right) {
        return (left.bigInteger <= right.bigInteger);
    }

    public static BigNumber Min(BigNumber left, BigNumber right) {
        return new BigNumber(Mathd.Min(left.bigInteger, right.bigInteger), unscaled: true);
    }

    public static BigNumber Min(params BigNumber[] values) {
        int length = values.Length;
        if(length == 0)
            return 0;
        BigNumber num = values[0];
        for(int index = 1; index < length; ++index) {
            if(values[index] < num)
                num = values[index];
        }
        return num;
    }

    public static BigNumber Max(BigNumber left, BigNumber right) {
        return new BigNumber(Mathd.Max(left.bigInteger, right.bigInteger), unscaled: true);
    }

    public static BigNumber Max(params BigNumber[] values) {
        int length = values.Length;
        if(length == 0)
            return 0;
        BigNumber num = values[0];
        for(int index = 1; index < length; ++index) {
            if(values[index] > num)
                num = values[index];
        }
        return num;
    }

    public static BigNumber Abs(BigNumber number) {
        if(number < 0)
            return -number;
        return number;
    }

    internal static BigNumber Round(BigNumber number) {
        var mod = new BigNumber((number.bigInteger + DECIMAL_SCALE / 2) % DECIMAL_SCALE, unscaled: true);
        return (number - mod);
    }

    public static BigNumber Floor(BigNumber number) {
        var mod = new BigNumber(number.bigInteger % DECIMAL_SCALE, unscaled: true);
        return (number - mod);
    }

    public static BigNumber Ceil(BigNumber number) {
        double intMod = (number.bigInteger % DECIMAL_SCALE);
        if(intMod == 0)
            return number;
        var mod = new BigNumber(DECIMAL_SCALE - intMod, unscaled: true);
        return (number + mod);
    }

    public static BigNumber Clamp(BigNumber number, BigNumber minimum, BigNumber maximum) {
        if(number < minimum)
            return minimum;
        if(number > maximum)
            return maximum;
        return number;
    }

    public override string ToString() {
        if(Magnitude >= 3)
            return ToStringWholeNumber();
        string fullNumber = Mathd.Abs(Mathd.Floor(bigInteger * 1000 / DECIMAL_SCALE)).ToString();
        while(fullNumber.Length < 4)
            fullNumber = "0" + fullNumber;
        int stringLength = ((fullNumber.Length - 4) % 3) + 4;
        fullNumber = fullNumber.Substring(0, stringLength);
        return (bigInteger < 0 ? "-" : "") + MagnitudinalNumber(fullNumber);
    }

    public string ToStringShorthand() {
        if(Magnitude >= 3)
            return ToStringWholeShorthand();
        string fullNumber = Mathd.Abs(Mathd.Floor(bigInteger * 1000 / DECIMAL_SCALE)).ToString();
        while(fullNumber.Length < 4)
            fullNumber = "0" + fullNumber;
        int stringLength = ((fullNumber.Length - 4) % 3) + 4;
        fullNumber = fullNumber.Substring(0, stringLength);
        return (bigInteger < 0 ? "-" : "") + MagnitudinalNumberShorthand(fullNumber);
    }

    public string ToStringWholeNumber() {
        string fullNumber = Mathd.Abs(Mathd.Floor(bigInteger / DECIMAL_SCALE)).ToString();
        if(fullNumber.Length <= 6)
            return (bigInteger < 0 ? "-" : "") + fullNumber;
        int stringLength = ((fullNumber.Length - 4) % 3) + 4;
        fullNumber = fullNumber.Substring(0, stringLength);
        return (bigInteger < 0 ? "-" : "") + MagnitudinalNumber(fullNumber);
    }

    public string ToStringWholeShorthand() {
        string fullNumber = Mathd.Abs(Mathd.Floor(bigInteger / DECIMAL_SCALE)).ToString();
        if(fullNumber.Length <= 6)
            return (bigInteger < 0 ? "-" : "") + fullNumber;
        int stringLength = ((fullNumber.Length - 4) % 3) + 4;
        fullNumber = fullNumber.Substring(0, stringLength);
        return (bigInteger < 0 ? "-" : "") + MagnitudinalNumberShorthand(fullNumber);
    }

    private string MagnitudinalNumber(string significantString) {
        string whole = significantString.Substring(0, significantString.Length - 3);
        string fraction = significantString.Substring(significantString.Length - 3, 3);
        if((Magnitude / 3) * 3 > 0)
            return whole + "." + fraction + " " + MagnitudeToTier((Magnitude / 3) * 3);
        return whole + "." + fraction;
    }

    private string MagnitudinalNumberShorthand(string significantString) {
        string whole = significantString.Substring(0, significantString.Length - 3);
        string fraction = significantString.Substring(significantString.Length - 3, 3);
        if((Magnitude / 3) * 3 > 0)
            return whole + "." + fraction + MagnitudeToShorthand((Magnitude / 3) * 3);
        return whole + "." + fraction;
    }

    private static string MagnitudeToTier(int magnitude) {
        if(magnitude >= 6 && magnitude < 6 + tiers.Length * 3)
            return tiers[(magnitude - 6) / 3];
        return "e" + magnitude;
    }

    private static string MagnitudeToShorthand(int magnitude) {
        if(magnitude >= 6 && magnitude < 6 + tierShorthand.Length * 3)
            return tierShorthand[(magnitude - 6) / 3];
        return "e" + magnitude;
    }

    public override bool Equals(object obj) {
        try {
            BigNumber bn = (BigNumber)obj;
            return (bn == this);
        }
        catch {
            return false;
        }
    }

    public override int GetHashCode() {
        return bigInteger.GetHashCode();
    }
}
/*/
using System;
using System.Globalization;
using System.Numerics;
using System.Text.Json.Serialization;

[Serializable]
public struct BigNumber {
    // Basically a BigInteger multiplied by a million to support fractions
    private const int DECIMAL_SCALE = 1000000000;
    private const int MAGNITUDE_SCALE = 9;

    private static readonly string[] TIERS = {
        "million", "billion", "trillion", "quadrillion", "quintillion", "sextillion", "septillion", "octillion", "nonillion", "decillion",
        "undecillion", "duodecillion", "tredecillion", "quattuordecillion", "quindecillion", "sexdecillion", "septendecillion", "octodecillion",
        "novemdecillion", "vigintillion"
    };

    private static readonly string[] TIER_SHORTHAND = {
        "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "O", "N", "D",
        "UD", "DD", "TD", "QaD", "QiD", "SxD", "SpD", "OD", "ND", "V"
    };

    private static readonly string[] UNIT_PREFIXES = {
        "un", "duo", "tre", "quattour", "quinqua", "se", "septe", "octo", "nove"
    };
    private static readonly string[] TENS_PREFIXES = {
        "deci", "viginti", "triginta", "quadraginta", "quinquaginta", "sexaginta", "septuaginta", "octoginta", "nonaginta"
    };
    private static readonly string[] TENS_MARKS = {
        "N", "MS", "NS", "NS", "NS", "N", "N", "MX", ""
    };
    private static readonly string[] HUNDREDS_PREFIXES = {
        "centi", "ducenti", "trecenti", "quadringenti", "quingenti", "sescenti", "septingenti", "octingenti", "nongenti"
    };
    private static readonly string[] HUNDREDS_MARKS = {
        "NX", "N", "NS", "NS", "NS", "N", "N", "MX", ""
    };

    [JsonPropertyName("BigInteger")]
    private BigInteger bigInteger;
    [JsonIgnore]
    private string bigString;

    public BigNumber(int integerValue) {
        bigInteger = new BigInteger(integerValue) * DECIMAL_SCALE;
        bigString = "";
    }

    public BigNumber(float floatValue) {
        try {
            bigInteger = new BigInteger(floatValue * DECIMAL_SCALE);
        }
        catch(OverflowException) {
            bigInteger = new BigInteger(floatValue) * DECIMAL_SCALE;
        }
        bigString = "";
    }

    public BigNumber(double doubleValue) {
        try {
            bigInteger = new BigInteger(doubleValue * DECIMAL_SCALE);
        }
        catch(OverflowException) {
            bigInteger = new BigInteger(doubleValue) * DECIMAL_SCALE;
        }
        bigString = "";
    }

    public BigNumber(BigInteger integerValue) {
        bigInteger = integerValue * DECIMAL_SCALE;
        bigString = "";
    }

    private BigNumber(BigInteger integerValue, bool unscaled = false) {
        if(unscaled)
            bigInteger = integerValue;
        else
            bigInteger = integerValue * DECIMAL_SCALE;
        bigString = "";
    }

    [JsonIgnore]
    public int Magnitude {
        get {
            if(BigInteger.Abs(bigInteger) < DECIMAL_SCALE)
                return 0;
            return BigInteger.Abs(bigInteger).ToString().Length - 1 - MAGNITUDE_SCALE;
        }
    }

    [JsonIgnore]
    public string StringRepresentation {
        get {
            var representation = bigInteger.ToString();
            int length = representation.Length;
            if(length > MAGNITUDE_SCALE) {
                representation = representation.Insert(length - MAGNITUDE_SCALE, ".");
            }
            else {
                if(length < MAGNITUDE_SCALE)
                    representation = new string('0', MAGNITUDE_SCALE - length) + representation;
                representation = "0." + representation;
            }
            if(representation.EndsWith(new string('0', MAGNITUDE_SCALE), StringComparison.Ordinal))
                representation = representation.Substring(0, representation.Length - 1 - MAGNITUDE_SCALE);
            return representation;
        }
    }

    public void OnBeforeSerialize() {
        bigString = StringRepresentation;
    }

    public void OnAfterDeserialize() {
        bigInteger = ((BigNumber)bigString).bigInteger;
    }

    public static implicit operator BigNumber(int number) {
        return new BigNumber(number);
    }

    public static implicit operator BigNumber(float number) {
        return new BigNumber(number);
    }

    public static implicit operator BigNumber(double number) {
        return new BigNumber(number);
    }

    public static implicit operator BigNumber(BigInteger number) {
        return new BigNumber(number);
    }

    public static implicit operator string(BigNumber number) {
        return number.ToString();
    }

    public static explicit operator BigNumber(string number) {
        if(number == "")
            number = "0";
        int indexOfPeriod = number.IndexOf('.');
        if(indexOfPeriod == -1)
            return new BigNumber(BigInteger.Parse(number, CultureInfo.InvariantCulture));
        int missingDigits = MAGNITUDE_SCALE + indexOfPeriod + 1 - number.Length;
        if(missingDigits > 0)
            number += new string('0', missingDigits);
        number = number.Substring(0, indexOfPeriod) + number.Substring(indexOfPeriod + 1, MAGNITUDE_SCALE);
        return new BigNumber(BigInteger.Parse(number, CultureInfo.InvariantCulture), true);
    }

    public static explicit operator float(BigNumber number) {
        return (float)number.bigInteger / DECIMAL_SCALE;
    }

    public static bool operator ==(BigNumber left, BigNumber right) {
        return left.bigInteger == right.bigInteger;
    }

    public static bool operator !=(BigNumber left, BigNumber right) {
        return left.bigInteger != right.bigInteger;
    }

    public static BigNumber operator +(BigNumber left, BigNumber right) {
        BigInteger sum = left.bigInteger + right.bigInteger;
        return new BigNumber(sum, true);
    }

    public static BigNumber operator ++(BigNumber num) {
        return num + 1;
    }

    public static BigNumber operator -(BigNumber number) {
        BigInteger newNumber = -number.bigInteger;
        return new BigNumber(newNumber, true);
    }

    public static BigNumber operator -(BigNumber left, BigNumber right) {
        BigInteger sum = left.bigInteger - right.bigInteger;
        return new BigNumber(sum, true);
    }

    public static BigNumber operator --(BigNumber num) {
        return num - 1;
    }

    public static BigNumber operator *(BigNumber left, BigNumber right) {
        BigInteger product = BigInteger.Multiply(left.bigInteger, right.bigInteger);
        BigInteger multipliedDown = product / DECIMAL_SCALE;
        return new BigNumber(multipliedDown, true);
    }

    public static BigNumber operator *(BigNumber bigNumber, double number) {
        BigInteger rightInteger = new(number * DECIMAL_SCALE * DECIMAL_SCALE);
        BigInteger product = BigInteger.Multiply(bigNumber.bigInteger, rightInteger);
        BigInteger multipliedDown = product / DECIMAL_SCALE / DECIMAL_SCALE;
        return new BigNumber(multipliedDown, true);
    }

    public static BigNumber operator /(BigNumber left, BigNumber right) {
        BigInteger result = BigInteger.Divide(left.bigInteger * DECIMAL_SCALE, right.bigInteger);
        return new BigNumber(result, true);
    }

    public static BigNumber operator /(BigNumber bigNumber, double number) {
        BigInteger rightInteger = new(number * DECIMAL_SCALE * DECIMAL_SCALE);
        BigInteger result = BigInteger.Divide(bigNumber.bigInteger * DECIMAL_SCALE * DECIMAL_SCALE, rightInteger);
        return new BigNumber(result, true);
    }

    public static bool operator >(BigNumber left, BigNumber right) {
        return left.bigInteger > right.bigInteger;
    }

    public static bool operator <(BigNumber left, BigNumber right) {
        return left.bigInteger < right.bigInteger;
    }

    public static bool operator >=(BigNumber left, BigNumber right) {
        return left.bigInteger >= right.bigInteger;
    }

    public static bool operator <=(BigNumber left, BigNumber right) {
        return left.bigInteger <= right.bigInteger;
    }

    public static BigNumber Min(BigNumber left, BigNumber right) {
        return new BigNumber(BigInteger.Min(left.bigInteger, right.bigInteger), true);
    }

    public static BigNumber Min(params BigNumber[] values) {
        int length = values.Length;
        if(length == 0)
            return 0;
        BigNumber num = values[0];
        for(var index = 1; index < length; ++index) {
            if(values[index] < num)
                num = values[index];
        }
        return num;
    }

    public static BigNumber Max(BigNumber left, BigNumber right) {
        return new BigNumber(BigInteger.Max(left.bigInteger, right.bigInteger), true);
    }

    public static BigNumber Max(params BigNumber[] values) {
        int length = values.Length;
        if(length == 0)
            return 0;
        BigNumber num = values[0];
        for(var index = 1; index < length; ++index) {
            if(values[index] > num)
                num = values[index];
        }
        return num;
    }

    public static BigNumber Abs(BigNumber number) {
        if(number < 0)
            return -number;
        return number;
    }

    public static BigNumber Round(BigNumber number) {
        BigInteger newInteger = number.bigInteger + DECIMAL_SCALE / 2;
        BigNumber mod = new(newInteger % DECIMAL_SCALE, true);
        return number - mod;
    }

    public static BigNumber Floor(BigNumber number) {
        BigNumber mod = new(number.bigInteger % DECIMAL_SCALE, true);
        return number - mod;
    }

    public static BigNumber Ceil(BigNumber number) {
        BigInteger intMod = number.bigInteger % DECIMAL_SCALE;
        if(intMod == 0)
            return number;
        BigNumber mod = new(DECIMAL_SCALE - intMod, true);
        return number + mod;
    }

    public static BigNumber Clamp(BigNumber number, BigNumber minimum,
                                  BigNumber maximum) {
        if(number < minimum)
            return minimum;
        if(number > maximum)
            return maximum;
        return number;
    }

    public static BigNumber Pow(BigNumber number, int exponent) {
        return new BigNumber(BigInteger.Pow(number.bigInteger, exponent) / BigInteger.Pow(DECIMAL_SCALE, exponent - 1), true);
    }

    public override string ToString() {
        if(Magnitude >= 3)
            return ToStringWholeNumber();
        var fullNumber = BigInteger.Abs(bigInteger * 1000 / DECIMAL_SCALE).ToString();
        while(fullNumber.Length < 4) {
            fullNumber = "0" + fullNumber;
        }
        int stringLength = (fullNumber.Length - 4) % 3 + 4;
        fullNumber = fullNumber.Substring(0, stringLength);
        return (bigInteger < 0 ? "-" : "") + MagnitudinalNumber(fullNumber);
    }

    public string ToStringShorthand() {
        if(Magnitude >= 3)
            return ToStringWholeShorthand();
        var fullNumber = BigInteger.Abs(bigInteger * 1000 / DECIMAL_SCALE).ToString();
        while(fullNumber.Length < 4) {
            fullNumber = "0" + fullNumber;
        }
        int stringLength = (fullNumber.Length - 4) % 3 + 4;
        fullNumber = fullNumber.Substring(0, stringLength);
        return (bigInteger < 0 ? "-" : "") + MagnitudinalNumberShorthand(fullNumber);
    }

    public string ToStringWholeNumber() {
        var fullNumber = BigInteger.Abs(bigInteger / DECIMAL_SCALE).ToString();
        if(fullNumber.Length <= 6)
            return (bigInteger < 0 ? "-" : "") + fullNumber;
        int stringLength = (fullNumber.Length - 4) % 3 + 4;
        fullNumber = fullNumber.Substring(0, stringLength);
        return (bigInteger < 0 ? "-" : "") + MagnitudinalNumber(fullNumber);
    }

    public string ToStringWholeShorthand() {
        var fullNumber = BigInteger.Abs(bigInteger / DECIMAL_SCALE).ToString();
        if(fullNumber.Length <= 6)
            return (bigInteger < 0 ? "-" : "") + fullNumber;
        int stringLength = (fullNumber.Length - 4) % 3 + 4;
        fullNumber = fullNumber.Substring(0, stringLength);
        return (bigInteger < 0 ? "-" : "") + MagnitudinalNumberShorthand(fullNumber);
    }

    private string MagnitudinalNumber(string significantString) {
        string whole = significantString.Substring(0, significantString.Length - 3);
        string fraction = significantString.Substring(significantString.Length - 3, 3);
        if(Magnitude / 3 * 3 > 0)
            return $"{whole}.{fraction} {MagnitudeToTier(Magnitude / 3 * 3)}";
        return $"{whole}.{fraction}";
    }

    private string MagnitudinalNumberShorthand(string significantString) {
        string whole = significantString.Substring(0, significantString.Length - 3);
        string fraction = significantString.Substring(significantString.Length - 3, 3);
        if(Magnitude / 3 * 3 > 0)
            return $"{whole}.{fraction}{MagnitudeToShorthand(Magnitude / 3 * 3)}";
        return $"{whole}.{fraction}";
    }

    private static string MagnitudeToTier(int magnitude) {
        if(magnitude == 0)
            return "";
        if(magnitude == 3)
            return "thousand";
        if(magnitude >= 6 && magnitude < 6 + TIERS.Length * 3)
            return TIERS[(magnitude - 6) / 3];
        if(magnitude < 3003)
            return ExponentComposition(magnitude);
        if(magnitude == 3003)
            return "millinillion";
        return "e" + magnitude;
    }

    private static string MagnitudeToShorthand(int magnitude) {
        if(magnitude >= 6 && magnitude < 6 + TIER_SHORTHAND.Length * 3)
            return TIER_SHORTHAND[(magnitude - 6) / 3];
        return "e" + magnitude;
    }

    /// <summary>
    /// Returns a name for a tens exponent between 9 and 3000.
    /// </summary>
    /// <param name="magnitude">The x in 10^x</param>
    /// <returns>Name of illion for x</returns>
    private static string ExponentComposition(int magnitude) {
        int baseIllion = magnitude / 3 - 1;
        int unitIllion = baseIllion % 10;
        int tensIllion = baseIllion / 10 % 10;
        int hundredsIllion = baseIllion / 100 % 10;
        var composition = "";
        if(unitIllion > 0) {
            composition += UNIT_PREFIXES[unitIllion - 1];
            var marks = "";
            if(tensIllion > 0)
                marks = TENS_MARKS[tensIllion - 1];
            else if(hundredsIllion > 0)
                marks = HUNDREDS_MARKS[hundredsIllion - 1];
            switch(unitIllion) {
            case 3:
                if(marks.Contains("S") || marks.Contains("X"))
                    composition += "s";
                break;
            case 6:
                if(marks.Contains("S"))
                    composition += "s";
                if(marks.Contains("X"))
                    composition += "x";
                break;
            case 7:
            case 9:
                if(marks.Contains("M"))
                    composition += "m";
                if(marks.Contains("N"))
                    composition += "n";
                break;
            }
        }
        if(tensIllion > 0)
            composition += TENS_PREFIXES[tensIllion - 1];
        if(hundredsIllion > 0)
            composition += HUNDREDS_PREFIXES[hundredsIllion - 1];
        return composition.Substring(0, composition.Length - 1) + "illion";
    }

    public override bool Equals(object? obj) {
        try {
            if(obj is BigNumber bn)
                return bn == this;
            return false;
        }
        catch {
            return false;
        }
    }

    public override int GetHashCode() {
        return bigInteger.GetHashCode();
    }
} //*/
