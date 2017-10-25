﻿using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
	public class NumberValidatorTests
	{
		[TestCase(17, 2, false, "0",
			ExpectedResult = true, TestName = "Succeeds_OnIntegerInBounds")]
		[TestCase(17, 2, false, "0.0",
			ExpectedResult = true, TestName = "Succeeds_OnFractionalInBounds")]
		[TestCase(17, 2, false, "-0.0",
			ExpectedResult = true, TestName = "Succeeds_OnNegativeFraction")]
		[TestCase(17, 0, false, "1",
			ExpectedResult = true, TestName = "Succeeds_OnIntegerWithZeroScale")]
		[TestCase(17, 0, false, "1.0",
			ExpectedResult = false, TestName = "Fails_OnFractionalWithZeroScale")]
		[TestCase(17, 2, true, "0.0",
			ExpectedResult = true, TestName = "Succeeds_OnPositiveInOnlyPositiveMode")]
		[TestCase(17, 2, true, "-0.0",
			ExpectedResult = false, TestName = "Fails_OnNegativeInOnlyPositiveMode")]
		[TestCase(17, 2, false, "0.000",
			ExpectedResult = false, TestName = "Fails_OnFractionBiggerThanScale")]
		[TestCase(3, 2, false, "0000",
			ExpectedResult = false, TestName = "Fails_OnNumberBiggerThanPrecision")]
		[TestCase(3, 2, false, "",
			ExpectedResult = false, TestName = "Fails_OnEmpty")]
		[TestCase(3, 2, false, null,
			ExpectedResult = false, TestName = "Fails_OnNull")]
		[TestCase(3, 2, false, "+11.1",
			ExpectedResult = false, TestName = "Fails_OnPositiveWithSignThatIsGreaterThanPrecision")]
		[TestCase(3, 2, false, "-11.1",
			ExpectedResult = false, TestName = "Fails_OnNegativeBecauseOfSing")]
		[TestCase(3, 2, false, "kek",
			ExpectedResult = false, TestName = "Fails_OnNonNumber")]
		[TestCase(30, 2, false, "10,1",
			ExpectedResult = true, TestName = "Succeeds_OnCommaSeparator")]
		[TestCase(30, 2, false, "10.1.1",
			ExpectedResult = false, TestName = "Fails_OnDoubleDot")]
		[TestCase(30, 2, false, "  10.1  ",
			ExpectedResult = false, TestName = "Fails_OnWhitespacesAround")]
		[TestCase(30, 2, false, "10.",
			ExpectedResult = false, TestName = "Fails_OnNoDigitAfterDot")]
		public bool TestIsValid(int precision, int scale, bool inlyPositve, string value)
		{
			return new NumberValidator(precision, scale, inlyPositve).IsValidNumber(value);
		}
		
		[TestCase(1, 2, TestName = "OnScaleGreaterThanPrecision")]
		[TestCase(2, 2, TestName = "OnScaleEqualToPrecision")]
		[TestCase(1, -2, TestName = "OnNegativeScale")]
		[TestCase(-1, 2, TestName = "OnNegativePrecision")]
		[TestCase(0, 2, TestName = "OnZeroPrecision")]
		public void TestConstructorThrowsArgumentException(int precision, int scale)
		{
			Assert.Throws<ArgumentException>(() => new NumberValidator(precision, scale));
		}

		[TestCase(3, 1, TestName = "OnPositvePrecisionGreaterThanPositiveScale")]
		[TestCase(3, 0, TestName = "OnPositvePrecisionGreaterThanZeroScale")]
		public void TestConstructorDoesNotThrowException(int precision, int scale)
		{
			Assert.DoesNotThrow(() => new NumberValidator(precision, scale));
		}
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