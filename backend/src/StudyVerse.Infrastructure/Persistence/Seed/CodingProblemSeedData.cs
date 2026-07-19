using System.Text.Json;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Infrastructure.Persistence.Seed;

/// <summary>
/// The static Coding Practice problem bank, applied via <c>CodingProblemConfiguration.HasData</c> /
/// <c>CodingProblemTestCaseConfiguration.HasData</c>. Ids are stable hardcoded GUIDs (pattern:
/// <c>66666666-6666-6666-6666-{sequence:D12}</c> for problems, and
/// <c>77777777-7777-7777-7777-{problemSequence:D4}{caseSequence:D8}</c> for test cases) so the seed
/// stays idempotent across migrations - the same reasoning as <c>QuizQuestionSeedData</c>.
///
/// 26 real, hand-written, verified-solvable problems (comfortably clearing the >=20 minimum) across
/// Easy/Medium/Hard and five categories (Arrays, Strings, Math, Recursion, Data Structures), each
/// with 4-5 real test cases (at least 2 marked <c>IsSample</c>), starter code for Python (Judge0 id
/// 109) and JavaScript/Node (Judge0 id 102), and every stdin/stdout pair worked out by hand (a few -
/// FizzBuzz, Two Sum, Valid Parentheses, Longest Palindromic Substring, Longest Common Subsequence,
/// Count Inversions, Longest Increasing Subsequence - were additionally cross-checked with a real
/// Judge0 call during this phase's verification pass; see the phase report). Five problems are
/// flagged <see cref="CodingProblem.IsInterviewQuestion"/>: Two Sum, Valid Parentheses, Binary
/// Search, Reverse a Linked List (adapted to a plain stdin/stdout array shape), and Merge Two
/// Sorted Arrays.
/// </summary>
public static class CodingProblemSeedData
{
    // Fixed (not DateTime.UtcNow) because EF Core HasData values must be static/deterministic - a
    // changing CreatedAtUtc would produce a spurious migration diff on every `migrations add`.
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public const string CategoryArrays = "Arrays";
    public const string CategoryStrings = "Strings";
    public const string CategoryMath = "Math";
    public const string CategoryRecursion = "Recursion";
    public const string CategoryDataStructures = "Data Structures";

    public static IReadOnlyList<string> Categories { get; } =
        [CategoryArrays, CategoryStrings, CategoryMath, CategoryRecursion, CategoryDataStructures];

    public static IReadOnlyList<CodingProblem> Problems { get; } = BuildProblems();

    public static IReadOnlyList<CodingProblemTestCase> TestCases { get; } = BuildTestCases();

    private static Guid ProblemId(int seq) => Guid.Parse($"66666666-6666-6666-6666-{seq:D12}");

    private static CodingProblem P(
        int seq,
        string title,
        string description,
        CodingDifficulty difficulty,
        string category,
        bool isInterview,
        string pythonStarter,
        string jsStarter) => new()
        {
            Id = ProblemId(seq),
            Title = title,
            Description = description,
            Difficulty = difficulty,
            Category = category,
            IsInterviewQuestion = isInterview,
            StarterCodeJson = JsonSerializer.Serialize(
                new Dictionary<string, string> { ["109"] = pythonStarter, ["102"] = jsStarter },
                JsonOptions),
            CreatedAtUtc = SeedTimestamp,
        };

    private static CodingProblemTestCase T(int problemSeq, int caseSeq, string input, string expectedOutput, bool isSample) => new()
    {
        Id = Guid.Parse($"77777777-7777-7777-7777-{problemSeq:D4}{caseSeq:D8}"),
        ProblemId = ProblemId(problemSeq),
        Input = input,
        ExpectedOutput = expectedOutput,
        IsSample = isSample,
        OrderIndex = caseSeq,
    };

    private static List<CodingProblem> BuildProblems() =>
    [
        // ---------------- Easy (11) ----------------
        P(1, "FizzBuzz",
            "Read a single integer n from stdin. For each integer i from 1 to n (inclusive), print " +
            "\"FizzBuzz\" if i is divisible by both 3 and 5, \"Fizz\" if divisible by 3 only, \"Buzz\" " +
            "if divisible by 5 only, otherwise print i itself. Print each result on its own line.",
            CodingDifficulty.Easy, CategoryMath, false,
            "n = int(input())\n# Write your solution: print Fizz/Buzz/FizzBuzz/i, one result per line, for i in 1..n\n",
            "const n = parseInt(require('fs').readFileSync(0, 'utf8').trim(), 10);\n" +
            "// Write your solution: print Fizz/Buzz/FizzBuzz/i, one result per line, for i in 1..n\n"),

        P(2, "Reverse a String",
            "Read a single line of text from stdin and print it reversed (character order reversed), " +
            "preserving every character exactly.",
            CodingDifficulty.Easy, CategoryStrings, false,
            "s = input()\n# Write your solution: print s reversed\n",
            "const s = require('fs').readFileSync(0, 'utf8').trim();\n// Write your solution: print s reversed\n"),

        P(3, "Count Vowels",
            "Read a single line of text from stdin and print the number of vowels (a, e, i, o, u - " +
            "case-insensitive) it contains.",
            CodingDifficulty.Easy, CategoryStrings, false,
            "s = input()\n# Write your solution: print the number of vowels in s (case-insensitive)\n",
            "const s = require('fs').readFileSync(0, 'utf8').trim();\n" +
            "// Write your solution: print the number of vowels in s (case-insensitive)\n"),

        P(4, "Sum of an Array",
            "Read a line of space-separated integers from stdin and print their sum.",
            CodingDifficulty.Easy, CategoryArrays, false,
            "arr = list(map(int, input().split()))\n# Write your solution: print the sum of arr\n",
            "const arr = require('fs').readFileSync(0, 'utf8').trim().split(' ').map(Number);\n" +
            "// Write your solution: print the sum of arr\n"),

        P(5, "Palindrome Check",
            "Read a single line of text from stdin (letters only, case-sensitive) and print \"YES\" if " +
            "it reads the same forwards and backwards, otherwise print \"NO\".",
            CodingDifficulty.Easy, CategoryStrings, false,
            "s = input()\n# Write your solution: print YES if s is a palindrome, else NO\n",
            "const s = require('fs').readFileSync(0, 'utf8').trim();\n" +
            "// Write your solution: print YES if s is a palindrome, else NO\n"),

        P(6, "Maximum of Three Numbers",
            "Read three space-separated integers from stdin and print the largest of the three.",
            CodingDifficulty.Easy, CategoryMath, false,
            "a, b, c = map(int, input().split())\n# Write your solution: print the largest of a, b, c\n",
            "const [a, b, c] = require('fs').readFileSync(0, 'utf8').trim().split(' ').map(Number);\n" +
            "// Write your solution: print the largest of a, b, c\n"),

        P(7, "Factorial of a Number",
            "Read a single non-negative integer n from stdin and print n! (n factorial). By " +
            "definition, 0! = 1.",
            CodingDifficulty.Easy, CategoryRecursion, false,
            "n = int(input())\n# Write your solution: print n factorial\n",
            "const n = parseInt(require('fs').readFileSync(0, 'utf8').trim(), 10);\n" +
            "// Write your solution: print n factorial\n"),

        P(8, "Find the Maximum Element in an Array",
            "Read a line of space-separated integers from stdin and print the largest value.",
            CodingDifficulty.Easy, CategoryArrays, false,
            "arr = list(map(int, input().split()))\n# Write your solution: print the largest value in arr\n",
            "const arr = require('fs').readFileSync(0, 'utf8').trim().split(' ').map(Number);\n" +
            "// Write your solution: print the largest value in arr\n"),

        P(9, "Count Words in a Sentence",
            "Read a single line of text from stdin and print the number of words in it, where words " +
            "are separated by single spaces.",
            CodingDifficulty.Easy, CategoryStrings, false,
            "s = input()\n# Write your solution: print the number of words in s\n",
            "const s = require('fs').readFileSync(0, 'utf8').trim();\n" +
            "// Write your solution: print the number of words in s\n"),

        P(10, "Merge Two Sorted Arrays",
            "Read three lines: the first contains two integers n and m (the sizes of the two arrays); " +
            "the second contains n space-separated integers already sorted in non-decreasing order; " +
            "the third contains m space-separated integers already sorted in non-decreasing order. " +
            "Print the merged array of all n+m integers in non-decreasing order, space-separated on " +
            "one line.",
            CodingDifficulty.Easy, CategoryArrays, true,
            "n, m = map(int, input().split())\n" +
            "a = list(map(int, input().split()))\n" +
            "b = list(map(int, input().split()))\n" +
            "# Write your solution: print a and b merged into one non-decreasing, space-separated list\n",
            "const lines = require('fs').readFileSync(0, 'utf8').trim().split('\\n');\n" +
            "const [n, m] = lines[0].split(' ').map(Number);\n" +
            "const a = lines[1].split(' ').map(Number);\n" +
            "const b = lines[2].split(' ').map(Number);\n" +
            "// Write your solution: print a and b merged into one non-decreasing, space-separated list\n"),

        P(11, "Check Prime Number",
            "Read a single integer n from stdin and print \"YES\" if n is a prime number, otherwise " +
            "print \"NO\". Numbers less than 2 are not prime.",
            CodingDifficulty.Easy, CategoryMath, false,
            "n = int(input())\n# Write your solution: print YES if n is prime, else NO\n",
            "const n = parseInt(require('fs').readFileSync(0, 'utf8').trim(), 10);\n" +
            "// Write your solution: print YES if n is prime, else NO\n"),

        // ---------------- Medium (10) ----------------
        P(12, "Two Sum",
            "Read a line containing an integer n (the array size), a second line with n " +
            "space-separated integers, and a third line with a single integer target. Print the two " +
            "0-based indices i and j (i < j, space-separated, i first) such that array[i] + array[j] " +
            "equals target. Assume exactly one valid pair exists.",
            CodingDifficulty.Medium, CategoryArrays, true,
            "n = int(input())\n" +
            "arr = list(map(int, input().split()))\n" +
            "target = int(input())\n" +
            "# Write your solution: print the two 0-based indices i j (i < j) with arr[i] + arr[j] == target\n",
            "const lines = require('fs').readFileSync(0, 'utf8').trim().split('\\n');\n" +
            "const n = parseInt(lines[0], 10);\n" +
            "const arr = lines[1].split(' ').map(Number);\n" +
            "const target = parseInt(lines[2], 10);\n" +
            "// Write your solution: print the two 0-based indices i j (i < j) with arr[i] + arr[j] === target\n"),

        P(13, "Valid Parentheses",
            "Read a single line containing a string made only of the characters '(', ')', '{', '}', " +
            "'[' and ']'. Print \"YES\" if the brackets are balanced and correctly nested, otherwise " +
            "print \"NO\".",
            CodingDifficulty.Medium, CategoryDataStructures, true,
            "s = input()\n# Write your solution: print YES if the brackets in s are balanced, else NO\n",
            "const s = require('fs').readFileSync(0, 'utf8').trim();\n" +
            "// Write your solution: print YES if the brackets in s are balanced, else NO\n"),

        P(14, "Binary Search",
            "Read a line with integer n, a second line with n space-separated integers already sorted " +
            "in strictly increasing order, and a third line with an integer target. Using binary " +
            "search, print the 0-based index of target in the array, or -1 if it is not present.",
            CodingDifficulty.Medium, CategoryArrays, true,
            "n = int(input())\n" +
            "arr = list(map(int, input().split()))\n" +
            "target = int(input())\n" +
            "# Write your solution: binary search for target in arr; print its 0-based index or -1\n",
            "const lines = require('fs').readFileSync(0, 'utf8').trim().split('\\n');\n" +
            "const n = parseInt(lines[0], 10);\n" +
            "const arr = lines[1].split(' ').map(Number);\n" +
            "const target = parseInt(lines[2], 10);\n" +
            "// Write your solution: binary search for target in arr; print its 0-based index or -1\n"),

        P(15, "Anagram Check",
            "Read two lines of text from stdin. Print \"YES\" if the second line is an anagram of the " +
            "first (same letters, same frequency, ignoring case and spaces), otherwise print \"NO\".",
            CodingDifficulty.Medium, CategoryStrings, false,
            "a = input()\nb = input()\n# Write your solution: print YES if b is an anagram of a, else NO\n",
            "const lines = require('fs').readFileSync(0, 'utf8').trim().split('\\n');\n" +
            "const [a, b] = lines;\n" +
            "// Write your solution: print YES if b is an anagram of a, else NO\n"),

        P(16, "Nth Fibonacci Number",
            "Read a single integer n (0-indexed, n >= 0) from stdin and print the nth Fibonacci " +
            "number, where fib(0) = 0, fib(1) = 1, and fib(n) = fib(n-1) + fib(n-2).",
            CodingDifficulty.Medium, CategoryRecursion, false,
            "n = int(input())\n# Write your solution: print the nth Fibonacci number\n",
            "const n = parseInt(require('fs').readFileSync(0, 'utf8').trim(), 10);\n" +
            "// Write your solution: print the nth Fibonacci number\n"),

        P(17, "Maximum Subarray Sum",
            "Read a line of space-separated integers (at least one) from stdin and print the maximum " +
            "possible sum of a contiguous subarray (Kadane's algorithm).",
            CodingDifficulty.Medium, CategoryArrays, false,
            "arr = list(map(int, input().split()))\n# Write your solution: print the maximum contiguous subarray sum\n",
            "const arr = require('fs').readFileSync(0, 'utf8').trim().split(' ').map(Number);\n" +
            "// Write your solution: print the maximum contiguous subarray sum\n"),

        P(18, "Reverse Words in a Sentence",
            "Read a single line of text (words separated by single spaces) from stdin and print the " +
            "words in reverse order, separated by single spaces.",
            CodingDifficulty.Medium, CategoryStrings, false,
            "s = input()\n# Write your solution: print the words of s in reverse order\n",
            "const s = require('fs').readFileSync(0, 'utf8').trim();\n" +
            "// Write your solution: print the words of s in reverse order\n"),

        P(19, "Second Largest Element in an Array",
            "Read a line of space-separated integers (at least two distinct values) from stdin and " +
            "print the second largest distinct value.",
            CodingDifficulty.Medium, CategoryArrays, false,
            "arr = list(map(int, input().split()))\n# Write your solution: print the second largest distinct value in arr\n",
            "const arr = require('fs').readFileSync(0, 'utf8').trim().split(' ').map(Number);\n" +
            "// Write your solution: print the second largest distinct value in arr\n"),

        P(20, "Reverse a Linked List",
            "A singly linked list is represented as a line of space-separated integers from stdin " +
            "(its node values, head to tail). Read that line, reverse the list, and print the node " +
            "values from the new head to tail, space-separated on one line.",
            CodingDifficulty.Medium, CategoryDataStructures, true,
            "values = list(map(int, input().split()))\n# Write your solution: print values reversed, space-separated\n",
            "const values = require('fs').readFileSync(0, 'utf8').trim().split(' ').map(Number);\n" +
            "// Write your solution: print values reversed, space-separated\n"),

        P(21, "Greatest Common Divisor",
            "Read two space-separated positive integers from stdin and print their greatest common " +
            "divisor (GCD).",
            CodingDifficulty.Medium, CategoryMath, false,
            "a, b = map(int, input().split())\n# Write your solution: print the GCD of a and b\n",
            "const [a, b] = require('fs').readFileSync(0, 'utf8').trim().split(' ').map(Number);\n" +
            "// Write your solution: print the GCD of a and b\n"),

        // ---------------- Hard (5) ----------------
        P(22, "Longest Palindromic Substring",
            "Read a single line of text from stdin containing only lowercase letters and print its " +
            "longest palindromic substring. If multiple substrings share the maximum length, print " +
            "the one that starts earliest in the string.",
            CodingDifficulty.Hard, CategoryStrings, false,
            "s = input()\n# Write your solution: print the longest palindromic substring of s (earliest start wins ties)\n",
            "const s = require('fs').readFileSync(0, 'utf8').trim();\n" +
            "// Write your solution: print the longest palindromic substring of s (earliest start wins ties)\n"),

        P(23, "Longest Common Subsequence",
            "Read two lines of text from stdin (two strings, lowercase letters only). Print the length " +
            "of their longest common subsequence.",
            CodingDifficulty.Hard, CategoryRecursion, false,
            "a = input()\nb = input()\n# Write your solution: print the length of the LCS of a and b\n",
            "const lines = require('fs').readFileSync(0, 'utf8').trim().split('\\n');\n" +
            "const [a, b] = lines;\n// Write your solution: print the length of the LCS of a and b\n"),

        P(24, "Kth Smallest Element in an Array",
            "Read a line with integer n, a second line with n space-separated integers, and a third " +
            "line with an integer k (1-indexed). Print the kth smallest element in the array.",
            CodingDifficulty.Hard, CategoryDataStructures, false,
            "n = int(input())\n" +
            "arr = list(map(int, input().split()))\n" +
            "k = int(input())\n" +
            "# Write your solution: print the kth smallest element of arr (k is 1-indexed)\n",
            "const lines = require('fs').readFileSync(0, 'utf8').trim().split('\\n');\n" +
            "const n = parseInt(lines[0], 10);\n" +
            "const arr = lines[1].split(' ').map(Number);\n" +
            "const k = parseInt(lines[2], 10);\n" +
            "// Write your solution: print the kth smallest element of arr (k is 1-indexed)\n"),

        P(25, "Count Inversions in an Array",
            "Read a line with integer n and a second line with n space-separated integers. An " +
            "inversion is a pair of indices i < j such that array[i] > array[j]. Print the total " +
            "number of inversions.",
            CodingDifficulty.Hard, CategoryArrays, false,
            "n = int(input())\n" +
            "arr = list(map(int, input().split()))\n" +
            "# Write your solution: print the number of inversions in arr\n",
            "const lines = require('fs').readFileSync(0, 'utf8').trim().split('\\n');\n" +
            "const n = parseInt(lines[0], 10);\n" +
            "const arr = lines[1].split(' ').map(Number);\n" +
            "// Write your solution: print the number of inversions in arr\n"),

        P(26, "Longest Increasing Subsequence",
            "Read a line with integer n and a second line with n space-separated integers. Print the " +
            "length of the longest strictly increasing subsequence.",
            CodingDifficulty.Hard, CategoryRecursion, false,
            "n = int(input())\n" +
            "arr = list(map(int, input().split()))\n" +
            "# Write your solution: print the length of the longest strictly increasing subsequence of arr\n",
            "const lines = require('fs').readFileSync(0, 'utf8').trim().split('\\n');\n" +
            "const n = parseInt(lines[0], 10);\n" +
            "const arr = lines[1].split(' ').map(Number);\n" +
            "// Write your solution: print the length of the longest strictly increasing subsequence of arr\n"),
    ];

    private static List<CodingProblemTestCase> BuildTestCases() =>
    [
        // 1. FizzBuzz
        T(1, 1, "5", "1\n2\nFizz\n4\nBuzz", true),
        T(1, 2, "15", "1\n2\nFizz\n4\nBuzz\nFizz\n7\n8\nFizz\nBuzz\n11\nFizz\n13\n14\nFizzBuzz", true),
        T(1, 3, "3", "1\n2\nFizz", false),
        T(1, 4, "1", "1", false),

        // 2. Reverse a String
        T(2, 1, "hello", "olleh", true),
        T(2, 2, "StudyVerse", "esreVydutS", true),
        T(2, 3, "a", "a", false),
        T(2, 4, "racecar", "racecar", false),

        // 3. Count Vowels
        T(3, 1, "Hello World", "3", true),
        T(3, 2, "AEIOUaeiou", "10", true),
        T(3, 3, "xyz", "0", false),
        T(3, 4, "StudyVerse AI", "5", false),

        // 4. Sum of an Array
        T(4, 1, "1 2 3 4 5", "15", true),
        T(4, 2, "-1 -2 -3", "-6", true),
        T(4, 3, "10", "10", false),
        T(4, 4, "0 0 0 0", "0", false),

        // 5. Palindrome Check
        T(5, 1, "racecar", "YES", true),
        T(5, 2, "hello", "NO", true),
        T(5, 3, "level", "YES", false),
        T(5, 4, "world", "NO", false),

        // 6. Maximum of Three Numbers
        T(6, 1, "3 7 5", "7", true),
        T(6, 2, "-1 -5 -3", "-1", true),
        T(6, 3, "10 10 5", "10", false),
        T(6, 4, "0 0 0", "0", false),

        // 7. Factorial of a Number
        T(7, 1, "5", "120", true),
        T(7, 2, "0", "1", true),
        T(7, 3, "1", "1", false),
        T(7, 4, "7", "5040", false),

        // 8. Find the Maximum Element in an Array
        T(8, 1, "4 8 2 9 3", "9", true),
        T(8, 2, "-5 -1 -10", "-1", true),
        T(8, 3, "7", "7", false),
        T(8, 4, "1 2 3 4 5", "5", false),

        // 9. Count Words in a Sentence
        T(9, 1, "The quick brown fox jumps", "5", true),
        T(9, 2, "Hello", "1", true),
        T(9, 3, "StudyVerse AI is great", "4", false),
        T(9, 4, "a b c d e f", "6", false),

        // 10. Merge Two Sorted Arrays
        T(10, 1, "3 3\n1 3 5\n2 4 6", "1 2 3 4 5 6", true),
        T(10, 2, "1 1\n5\n5", "5 5", true),
        T(10, 3, "4 2\n1 2 3 4\n0 5", "0 1 2 3 4 5", false),
        T(10, 4, "3 2\n-3 -1 2\n-2 0", "-3 -2 -1 0 2", false),

        // 11. Check Prime Number
        T(11, 1, "7", "YES", true),
        T(11, 2, "10", "NO", true),
        T(11, 3, "2", "YES", false),
        T(11, 4, "1", "NO", false),
        T(11, 5, "97", "YES", false),

        // 12. Two Sum
        T(12, 1, "4\n2 7 11 15\n9", "0 1", true),
        T(12, 2, "3\n3 2 4\n6", "1 2", true),
        T(12, 3, "2\n3 3\n6", "0 1", false),
        T(12, 4, "5\n1 5 3 8 2\n11", "2 3", false),

        // 13. Valid Parentheses
        T(13, 1, "()[]{}", "YES", true),
        T(13, 2, "(]", "NO", true),
        T(13, 3, "{[()()]}", "YES", false),
        T(13, 4, "([)]", "NO", false),
        T(13, 5, "(((", "NO", false),

        // 14. Binary Search
        T(14, 1, "5\n1 3 5 7 9\n7", "3", true),
        T(14, 2, "5\n1 3 5 7 9\n4", "-1", true),
        T(14, 3, "1\n10\n10", "0", false),
        T(14, 4, "6\n2 4 6 8 10 12\n2", "0", false),
        T(14, 5, "6\n2 4 6 8 10 12\n12", "5", false),

        // 15. Anagram Check
        T(15, 1, "listen\nsilent", "YES", true),
        T(15, 2, "hello\nworld", "NO", true),
        T(15, 3, "Dormitory\nDirty Room", "YES", false),
        T(15, 4, "abc\nabd", "NO", false),

        // 16. Nth Fibonacci Number
        T(16, 1, "0", "0", true),
        T(16, 2, "1", "1", true),
        T(16, 3, "10", "55", false),
        T(16, 4, "20", "6765", false),

        // 17. Maximum Subarray Sum
        T(17, 1, "-2 1 -3 4 -1 2 1 -5 4", "6", true),
        T(17, 2, "1 2 3 4", "10", true),
        T(17, 3, "-1 -2 -3", "-1", false),
        T(17, 4, "5", "5", false),

        // 18. Reverse Words in a Sentence
        T(18, 1, "The sky is blue", "blue is sky The", true),
        T(18, 2, "Hello World", "World Hello", true),
        T(18, 3, "one two three four five", "five four three two one", false),
        T(18, 4, "StudyVerse", "StudyVerse", false),

        // 19. Second Largest Element in an Array
        T(19, 1, "4 8 2 9 3", "8", true),
        T(19, 2, "1 1 2 2 3 3", "2", true),
        T(19, 3, "10 20", "10", false),
        T(19, 4, "5 5 5 4", "4", false),

        // 20. Reverse a Linked List
        T(20, 1, "1 2 3 4 5", "5 4 3 2 1", true),
        T(20, 2, "10 20", "20 10", true),
        T(20, 3, "7", "7", false),
        T(20, 4, "1 2 3 4 5 6 7 8", "8 7 6 5 4 3 2 1", false),

        // 21. Greatest Common Divisor
        T(21, 1, "12 18", "6", true),
        T(21, 2, "17 5", "1", true),
        T(21, 3, "100 75", "25", false),
        T(21, 4, "7 7", "7", false),

        // 22. Longest Palindromic Substring
        T(22, 1, "babad", "bab", true),
        T(22, 2, "cbbd", "bb", true),
        T(22, 3, "a", "a", false),
        T(22, 4, "forgeeksskeegfor", "geeksskeeg", false),

        // 23. Longest Common Subsequence
        T(23, 1, "abcde\nace", "3", true),
        T(23, 2, "abc\nabc", "3", true),
        T(23, 3, "abc\ndef", "0", false),
        T(23, 4, "abcba\nabcbcba", "5", false),

        // 24. Kth Smallest Element in an Array
        T(24, 1, "5\n7 10 4 3 20\n3", "7", true),
        T(24, 2, "4\n1 2 2 3\n2", "2", true),
        T(24, 3, "1\n5\n1", "5", false),
        T(24, 4, "6\n9 8 7 6 5 4\n1", "4", false),

        // 25. Count Inversions in an Array
        T(25, 1, "5\n2 4 1 3 5", "3", true),
        T(25, 2, "4\n1 2 3 4", "0", true),
        T(25, 3, "4\n4 3 2 1", "6", false),
        T(25, 4, "1\n5", "0", false),

        // 26. Longest Increasing Subsequence
        T(26, 1, "8\n10 9 2 5 3 7 101 18", "4", true),
        T(26, 2, "1\n7", "1", true),
        T(26, 3, "4\n4 3 2 1", "1", false),
        T(26, 4, "6\n1 2 3 4 5 6", "6", false),
    ];
}
