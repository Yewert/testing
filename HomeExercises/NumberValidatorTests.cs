using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
	public class NumberValidatorTests
	{
		[TestCase(17, 2, false, "0",
			ExpectedResult = true, TestName = "IntegerInBounds")]
		[TestCase(17, 2, false, "0.0",
			ExpectedResult = true, TestName = "FractionalInBounds")]
		[TestCase(17, 2, false, "-0.0",
			ExpectedResult = true, TestName = "NegativeFraction")]
		[TestCase(17, 0, false, "1",
			ExpectedResult = true, TestName = "IntegerInWithZeroScale")]
		[TestCase(17, 0, false, "1.0",
			ExpectedResult = false, TestName = "Fails_On_FractionalWithZeroScale")]
		[TestCase(17, 2, true, "0.0",
			ExpectedResult = true, TestName = "PositiveInOnlyPositiveMode")]
		[TestCase(17, 2, true, "-0.0",
			ExpectedResult = false, TestName = "Fails_On_NegativeInOnlyPositiveMode")]
		[TestCase(17, 2, false, "0.000",
			ExpectedResult = false, TestName = "Fails_On_FractionBiggerThanScale")]
		[TestCase(3, 2, false, "0000",
			ExpectedResult = false, TestName = "Fails_On_NumberBiggerThanPrecision")]
		[TestCase(3, 2, false, "",
			ExpectedResult = false, TestName = "Fails_On_Empty")]
		[TestCase(3, 2, false, null,
			ExpectedResult = false, TestName = "Fails_On_Null")]
		[TestCase(3, 2, false, "+11.1",
			ExpectedResult = false, TestName = "Fails_On_PositiveWithSignThatIsGreaterThanPrecision")]
		[TestCase(3, 2, false, "-11.1",
			ExpectedResult = false, TestName = "Fails_On_NegativeBecauseOfSing")]
		[TestCase(3, 2, false, "kek",
			ExpectedResult = false, TestName = "Fails_On_NonNumber")]
		[TestCase(30, 2, false, "10,1",
			ExpectedResult = true, TestName = "CommaSeparator")]
		[TestCase(30, 2, false, "10.1.1",
			ExpectedResult = false, TestName = "Fails_On_DoubleDot")]
		[TestCase(30, 2, false, "  10.1  ",
			ExpectedResult = false, TestName = "Fails_On_WhitespacesAround")]
		[TestCase(30, 2, false, "10.",
			ExpectedResult = false, TestName = "Fails_On_NoDigitAfterDot")]
		public bool TestWithoutExceptions(int precision, int scale, bool unsigned, string value)
		{
			return new NumberValidator(precision, scale, unsigned).IsValidNumber(value);
		}

		
		// В NUnit3 выпилили параметр ExpectedException из аттрибута TestCase :( 
		
		[Test]
		public void Fails_On_ScaleGreaterThanPrecision()
		{
			Assert.Throws<ArgumentException>(() => new NumberValidator(1, 2, true));
		}
		
		[Test]
		public void Fails_On_ScaleEqualToPrecision()
		{
			Assert.Throws<ArgumentException>(() => new NumberValidator(2, 2, true));
		}
		
		[Test]
		public void Fails_On_NegativeScale()
		{
			Assert.Throws<ArgumentException>(() => new NumberValidator(1, -2, true));
		}
		
		[Test]
		public void Fails_On_Negativeprecision()
		{
			Assert.Throws<ArgumentException>(() => new NumberValidator(-1, 2, true));
		}
		
		
//		//[Test]
//		public void Test()
//		{
//			Assert.Multiple(() =>
//			{
//				Assert.Throws<ArgumentException>(() => new NumberValidator(-1, 2, true));
//				Assert.Throws<ArgumentException>(() => new NumberValidator(1, 2, true));
//				
//				Assert.DoesNotThrow(() => new NumberValidator(1, 0, true));
//				//Assert.Throws<ArgumentException>(() => new NumberValidator(-1, 2, false));
//				//Assert.DoesNotThrow(() => new NumberValidator(1, 0, true));
//
//				Assert.IsTrue(new NumberValidator(17, 2, true).IsValidNumber("0.0"));
//				Assert.IsTrue(new NumberValidator(17, 2, true).IsValidNumber("0"));
//				//Assert.IsTrue(new NumberValidator(17, 2, true).IsValidNumber("0.0"));
//				Assert.IsFalse(new NumberValidator(3, 2, true).IsValidNumber("00.00"));
//				Assert.IsFalse(new NumberValidator(3, 2, true).IsValidNumber("-0.00"));
//				Assert.IsTrue(new NumberValidator(17, 2, true).IsValidNumber("0.0"));
//				//Assert.IsFalse(new NumberValidator(3, 2, true).IsValidNumber("+0.00"));
//				Assert.IsTrue(new NumberValidator(4, 2, true).IsValidNumber("+1.23"));
//				Assert.IsFalse(new NumberValidator(3, 2, true).IsValidNumber("+1.23"));
//				Assert.IsFalse(new NumberValidator(17, 2, true).IsValidNumber("0.000"));
//				Assert.IsFalse(new NumberValidator(3, 2, true).IsValidNumber("-1.23"));
//				Assert.IsFalse(new NumberValidator(3, 2, true).IsValidNumber("a.sd"));
//
//				Assert.IsTrue(new NumberValidator(1, 0, false).IsValidNumber("1"));
//				Assert.IsFalse(new NumberValidator(1, 0, false).IsValidNumber("-"));
//			});
//		}
	}

	public class NumberValidator
	{
		private readonly Regex numberRegex;
		private readonly bool onlyPositive;
		private readonly int precision;
		private readonly int scale;

		public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
		{
			this.precision = precision;
			this.scale = scale;
			this.onlyPositive = onlyPositive;
			if (precision <= 0)
				throw new ArgumentException("precision must be a positive number");
			if (scale < 0 || scale >= precision)
				throw new ArgumentException("precision must be a non-negative number less or equal than precision");
			numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
		}

		public bool IsValidNumber(string value)
		{
			// Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
			// описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
			// Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
			// целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
			// Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

			if (string.IsNullOrEmpty(value))
				return false;

			var match = numberRegex.Match(value);
			if (!match.Success)
				return false;

			// Знак и целая часть
			var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
			// Дробная часть
			var fracPart = match.Groups[4].Value.Length;

			if (intPart + fracPart > precision || fracPart > scale)
				return false;

			if (onlyPositive && match.Groups[1].Value == "-")
				return false;
			return true;
		}
	}
}