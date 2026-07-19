using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StudyVerse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCodingPracticeFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "coding_problems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Difficulty = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsInterviewQuestion = table.Column<bool>(type: "boolean", nullable: false),
                    StarterCodeJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coding_problems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "code_submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    SourceCode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TestsPassed = table.Column<int>(type: "integer", nullable: false),
                    TotalTests = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_code_submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_code_submissions_coding_problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "coding_problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_code_submissions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "coding_problem_test_cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Input = table.Column<string>(type: "text", nullable: false),
                    ExpectedOutput = table.Column<string>(type: "text", nullable: false),
                    IsSample = table.Column<bool>(type: "boolean", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coding_problem_test_cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_coding_problem_test_cases_coding_problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "coding_problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "coding_problems",
                columns: new[] { "Id", "Category", "CreatedAtUtc", "Description", "Difficulty", "IsInterviewQuestion", "StarterCodeJson", "Title" },
                values: new object[,]
                {
                    { new Guid("66666666-6666-6666-6666-000000000001"), "Math", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a single integer n from stdin. For each integer i from 1 to n (inclusive), print \"FizzBuzz\" if i is divisible by both 3 and 5, \"Fizz\" if divisible by 3 only, \"Buzz\" if divisible by 5 only, otherwise print i itself. Print each result on its own line.", "Easy", false, "{\"109\":\"n = int(input())\\n# Write your solution: print Fizz/Buzz/FizzBuzz/i, one result per line, for i in 1..n\\n\",\"102\":\"const n = parseInt(require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim(), 10);\\n// Write your solution: print Fizz/Buzz/FizzBuzz/i, one result per line, for i in 1..n\\n\"}", "FizzBuzz" },
                    { new Guid("66666666-6666-6666-6666-000000000002"), "Strings", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a single line of text from stdin and print it reversed (character order reversed), preserving every character exactly.", "Easy", false, "{\"109\":\"s = input()\\n# Write your solution: print s reversed\\n\",\"102\":\"const s = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim();\\n// Write your solution: print s reversed\\n\"}", "Reverse a String" },
                    { new Guid("66666666-6666-6666-6666-000000000003"), "Strings", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a single line of text from stdin and print the number of vowels (a, e, i, o, u - case-insensitive) it contains.", "Easy", false, "{\"109\":\"s = input()\\n# Write your solution: print the number of vowels in s (case-insensitive)\\n\",\"102\":\"const s = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim();\\n// Write your solution: print the number of vowels in s (case-insensitive)\\n\"}", "Count Vowels" },
                    { new Guid("66666666-6666-6666-6666-000000000004"), "Arrays", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a line of space-separated integers from stdin and print their sum.", "Easy", false, "{\"109\":\"arr = list(map(int, input().split()))\\n# Write your solution: print the sum of arr\\n\",\"102\":\"const arr = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim().split(\\u0027 \\u0027).map(Number);\\n// Write your solution: print the sum of arr\\n\"}", "Sum of an Array" },
                    { new Guid("66666666-6666-6666-6666-000000000005"), "Strings", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a single line of text from stdin (letters only, case-sensitive) and print \"YES\" if it reads the same forwards and backwards, otherwise print \"NO\".", "Easy", false, "{\"109\":\"s = input()\\n# Write your solution: print YES if s is a palindrome, else NO\\n\",\"102\":\"const s = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim();\\n// Write your solution: print YES if s is a palindrome, else NO\\n\"}", "Palindrome Check" },
                    { new Guid("66666666-6666-6666-6666-000000000006"), "Math", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read three space-separated integers from stdin and print the largest of the three.", "Easy", false, "{\"109\":\"a, b, c = map(int, input().split())\\n# Write your solution: print the largest of a, b, c\\n\",\"102\":\"const [a, b, c] = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim().split(\\u0027 \\u0027).map(Number);\\n// Write your solution: print the largest of a, b, c\\n\"}", "Maximum of Three Numbers" },
                    { new Guid("66666666-6666-6666-6666-000000000007"), "Recursion", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a single non-negative integer n from stdin and print n! (n factorial). By definition, 0! = 1.", "Easy", false, "{\"109\":\"n = int(input())\\n# Write your solution: print n factorial\\n\",\"102\":\"const n = parseInt(require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim(), 10);\\n// Write your solution: print n factorial\\n\"}", "Factorial of a Number" },
                    { new Guid("66666666-6666-6666-6666-000000000008"), "Arrays", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a line of space-separated integers from stdin and print the largest value.", "Easy", false, "{\"109\":\"arr = list(map(int, input().split()))\\n# Write your solution: print the largest value in arr\\n\",\"102\":\"const arr = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim().split(\\u0027 \\u0027).map(Number);\\n// Write your solution: print the largest value in arr\\n\"}", "Find the Maximum Element in an Array" },
                    { new Guid("66666666-6666-6666-6666-000000000009"), "Strings", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a single line of text from stdin and print the number of words in it, where words are separated by single spaces.", "Easy", false, "{\"109\":\"s = input()\\n# Write your solution: print the number of words in s\\n\",\"102\":\"const s = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim();\\n// Write your solution: print the number of words in s\\n\"}", "Count Words in a Sentence" },
                    { new Guid("66666666-6666-6666-6666-000000000010"), "Arrays", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read three lines: the first contains two integers n and m (the sizes of the two arrays); the second contains n space-separated integers already sorted in non-decreasing order; the third contains m space-separated integers already sorted in non-decreasing order. Print the merged array of all n+m integers in non-decreasing order, space-separated on one line.", "Easy", true, "{\"109\":\"n, m = map(int, input().split())\\na = list(map(int, input().split()))\\nb = list(map(int, input().split()))\\n# Write your solution: print a and b merged into one non-decreasing, space-separated list\\n\",\"102\":\"const lines = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim().split(\\u0027\\\\n\\u0027);\\nconst [n, m] = lines[0].split(\\u0027 \\u0027).map(Number);\\nconst a = lines[1].split(\\u0027 \\u0027).map(Number);\\nconst b = lines[2].split(\\u0027 \\u0027).map(Number);\\n// Write your solution: print a and b merged into one non-decreasing, space-separated list\\n\"}", "Merge Two Sorted Arrays" },
                    { new Guid("66666666-6666-6666-6666-000000000011"), "Math", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a single integer n from stdin and print \"YES\" if n is a prime number, otherwise print \"NO\". Numbers less than 2 are not prime.", "Easy", false, "{\"109\":\"n = int(input())\\n# Write your solution: print YES if n is prime, else NO\\n\",\"102\":\"const n = parseInt(require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim(), 10);\\n// Write your solution: print YES if n is prime, else NO\\n\"}", "Check Prime Number" },
                    { new Guid("66666666-6666-6666-6666-000000000012"), "Arrays", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a line containing an integer n (the array size), a second line with n space-separated integers, and a third line with a single integer target. Print the two 0-based indices i and j (i < j, space-separated, i first) such that array[i] + array[j] equals target. Assume exactly one valid pair exists.", "Medium", true, "{\"109\":\"n = int(input())\\narr = list(map(int, input().split()))\\ntarget = int(input())\\n# Write your solution: print the two 0-based indices i j (i \\u003C j) with arr[i] \\u002B arr[j] == target\\n\",\"102\":\"const lines = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim().split(\\u0027\\\\n\\u0027);\\nconst n = parseInt(lines[0], 10);\\nconst arr = lines[1].split(\\u0027 \\u0027).map(Number);\\nconst target = parseInt(lines[2], 10);\\n// Write your solution: print the two 0-based indices i j (i \\u003C j) with arr[i] \\u002B arr[j] === target\\n\"}", "Two Sum" },
                    { new Guid("66666666-6666-6666-6666-000000000013"), "Data Structures", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a single line containing a string made only of the characters '(', ')', '{', '}', '[' and ']'. Print \"YES\" if the brackets are balanced and correctly nested, otherwise print \"NO\".", "Medium", true, "{\"109\":\"s = input()\\n# Write your solution: print YES if the brackets in s are balanced, else NO\\n\",\"102\":\"const s = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim();\\n// Write your solution: print YES if the brackets in s are balanced, else NO\\n\"}", "Valid Parentheses" },
                    { new Guid("66666666-6666-6666-6666-000000000014"), "Arrays", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a line with integer n, a second line with n space-separated integers already sorted in strictly increasing order, and a third line with an integer target. Using binary search, print the 0-based index of target in the array, or -1 if it is not present.", "Medium", true, "{\"109\":\"n = int(input())\\narr = list(map(int, input().split()))\\ntarget = int(input())\\n# Write your solution: binary search for target in arr; print its 0-based index or -1\\n\",\"102\":\"const lines = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim().split(\\u0027\\\\n\\u0027);\\nconst n = parseInt(lines[0], 10);\\nconst arr = lines[1].split(\\u0027 \\u0027).map(Number);\\nconst target = parseInt(lines[2], 10);\\n// Write your solution: binary search for target in arr; print its 0-based index or -1\\n\"}", "Binary Search" },
                    { new Guid("66666666-6666-6666-6666-000000000015"), "Strings", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read two lines of text from stdin. Print \"YES\" if the second line is an anagram of the first (same letters, same frequency, ignoring case and spaces), otherwise print \"NO\".", "Medium", false, "{\"109\":\"a = input()\\nb = input()\\n# Write your solution: print YES if b is an anagram of a, else NO\\n\",\"102\":\"const lines = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim().split(\\u0027\\\\n\\u0027);\\nconst [a, b] = lines;\\n// Write your solution: print YES if b is an anagram of a, else NO\\n\"}", "Anagram Check" },
                    { new Guid("66666666-6666-6666-6666-000000000016"), "Recursion", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a single integer n (0-indexed, n >= 0) from stdin and print the nth Fibonacci number, where fib(0) = 0, fib(1) = 1, and fib(n) = fib(n-1) + fib(n-2).", "Medium", false, "{\"109\":\"n = int(input())\\n# Write your solution: print the nth Fibonacci number\\n\",\"102\":\"const n = parseInt(require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim(), 10);\\n// Write your solution: print the nth Fibonacci number\\n\"}", "Nth Fibonacci Number" },
                    { new Guid("66666666-6666-6666-6666-000000000017"), "Arrays", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a line of space-separated integers (at least one) from stdin and print the maximum possible sum of a contiguous subarray (Kadane's algorithm).", "Medium", false, "{\"109\":\"arr = list(map(int, input().split()))\\n# Write your solution: print the maximum contiguous subarray sum\\n\",\"102\":\"const arr = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim().split(\\u0027 \\u0027).map(Number);\\n// Write your solution: print the maximum contiguous subarray sum\\n\"}", "Maximum Subarray Sum" },
                    { new Guid("66666666-6666-6666-6666-000000000018"), "Strings", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a single line of text (words separated by single spaces) from stdin and print the words in reverse order, separated by single spaces.", "Medium", false, "{\"109\":\"s = input()\\n# Write your solution: print the words of s in reverse order\\n\",\"102\":\"const s = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim();\\n// Write your solution: print the words of s in reverse order\\n\"}", "Reverse Words in a Sentence" },
                    { new Guid("66666666-6666-6666-6666-000000000019"), "Arrays", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a line of space-separated integers (at least two distinct values) from stdin and print the second largest distinct value.", "Medium", false, "{\"109\":\"arr = list(map(int, input().split()))\\n# Write your solution: print the second largest distinct value in arr\\n\",\"102\":\"const arr = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim().split(\\u0027 \\u0027).map(Number);\\n// Write your solution: print the second largest distinct value in arr\\n\"}", "Second Largest Element in an Array" },
                    { new Guid("66666666-6666-6666-6666-000000000020"), "Data Structures", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "A singly linked list is represented as a line of space-separated integers from stdin (its node values, head to tail). Read that line, reverse the list, and print the node values from the new head to tail, space-separated on one line.", "Medium", true, "{\"109\":\"values = list(map(int, input().split()))\\n# Write your solution: print values reversed, space-separated\\n\",\"102\":\"const values = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim().split(\\u0027 \\u0027).map(Number);\\n// Write your solution: print values reversed, space-separated\\n\"}", "Reverse a Linked List" },
                    { new Guid("66666666-6666-6666-6666-000000000021"), "Math", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read two space-separated positive integers from stdin and print their greatest common divisor (GCD).", "Medium", false, "{\"109\":\"a, b = map(int, input().split())\\n# Write your solution: print the GCD of a and b\\n\",\"102\":\"const [a, b] = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim().split(\\u0027 \\u0027).map(Number);\\n// Write your solution: print the GCD of a and b\\n\"}", "Greatest Common Divisor" },
                    { new Guid("66666666-6666-6666-6666-000000000022"), "Strings", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a single line of text from stdin containing only lowercase letters and print its longest palindromic substring. If multiple substrings share the maximum length, print the one that starts earliest in the string.", "Hard", false, "{\"109\":\"s = input()\\n# Write your solution: print the longest palindromic substring of s (earliest start wins ties)\\n\",\"102\":\"const s = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim();\\n// Write your solution: print the longest palindromic substring of s (earliest start wins ties)\\n\"}", "Longest Palindromic Substring" },
                    { new Guid("66666666-6666-6666-6666-000000000023"), "Recursion", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read two lines of text from stdin (two strings, lowercase letters only). Print the length of their longest common subsequence.", "Hard", false, "{\"109\":\"a = input()\\nb = input()\\n# Write your solution: print the length of the LCS of a and b\\n\",\"102\":\"const lines = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim().split(\\u0027\\\\n\\u0027);\\nconst [a, b] = lines;\\n// Write your solution: print the length of the LCS of a and b\\n\"}", "Longest Common Subsequence" },
                    { new Guid("66666666-6666-6666-6666-000000000024"), "Data Structures", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a line with integer n, a second line with n space-separated integers, and a third line with an integer k (1-indexed). Print the kth smallest element in the array.", "Hard", false, "{\"109\":\"n = int(input())\\narr = list(map(int, input().split()))\\nk = int(input())\\n# Write your solution: print the kth smallest element of arr (k is 1-indexed)\\n\",\"102\":\"const lines = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim().split(\\u0027\\\\n\\u0027);\\nconst n = parseInt(lines[0], 10);\\nconst arr = lines[1].split(\\u0027 \\u0027).map(Number);\\nconst k = parseInt(lines[2], 10);\\n// Write your solution: print the kth smallest element of arr (k is 1-indexed)\\n\"}", "Kth Smallest Element in an Array" },
                    { new Guid("66666666-6666-6666-6666-000000000025"), "Arrays", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a line with integer n and a second line with n space-separated integers. An inversion is a pair of indices i < j such that array[i] > array[j]. Print the total number of inversions.", "Hard", false, "{\"109\":\"n = int(input())\\narr = list(map(int, input().split()))\\n# Write your solution: print the number of inversions in arr\\n\",\"102\":\"const lines = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim().split(\\u0027\\\\n\\u0027);\\nconst n = parseInt(lines[0], 10);\\nconst arr = lines[1].split(\\u0027 \\u0027).map(Number);\\n// Write your solution: print the number of inversions in arr\\n\"}", "Count Inversions in an Array" },
                    { new Guid("66666666-6666-6666-6666-000000000026"), "Recursion", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read a line with integer n and a second line with n space-separated integers. Print the length of the longest strictly increasing subsequence.", "Hard", false, "{\"109\":\"n = int(input())\\narr = list(map(int, input().split()))\\n# Write your solution: print the length of the longest strictly increasing subsequence of arr\\n\",\"102\":\"const lines = require(\\u0027fs\\u0027).readFileSync(0, \\u0027utf8\\u0027).trim().split(\\u0027\\\\n\\u0027);\\nconst n = parseInt(lines[0], 10);\\nconst arr = lines[1].split(\\u0027 \\u0027).map(Number);\\n// Write your solution: print the length of the longest strictly increasing subsequence of arr\\n\"}", "Longest Increasing Subsequence" }
                });

            migrationBuilder.InsertData(
                table: "coding_problem_test_cases",
                columns: new[] { "Id", "ExpectedOutput", "Input", "IsSample", "OrderIndex", "ProblemId" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-000100000001"), "1\n2\nFizz\n4\nBuzz", "5", true, 1, new Guid("66666666-6666-6666-6666-000000000001") },
                    { new Guid("77777777-7777-7777-7777-000100000002"), "1\n2\nFizz\n4\nBuzz\nFizz\n7\n8\nFizz\nBuzz\n11\nFizz\n13\n14\nFizzBuzz", "15", true, 2, new Guid("66666666-6666-6666-6666-000000000001") },
                    { new Guid("77777777-7777-7777-7777-000100000003"), "1\n2\nFizz", "3", false, 3, new Guid("66666666-6666-6666-6666-000000000001") },
                    { new Guid("77777777-7777-7777-7777-000100000004"), "1", "1", false, 4, new Guid("66666666-6666-6666-6666-000000000001") },
                    { new Guid("77777777-7777-7777-7777-000200000001"), "olleh", "hello", true, 1, new Guid("66666666-6666-6666-6666-000000000002") },
                    { new Guid("77777777-7777-7777-7777-000200000002"), "esreVydutS", "StudyVerse", true, 2, new Guid("66666666-6666-6666-6666-000000000002") },
                    { new Guid("77777777-7777-7777-7777-000200000003"), "a", "a", false, 3, new Guid("66666666-6666-6666-6666-000000000002") },
                    { new Guid("77777777-7777-7777-7777-000200000004"), "racecar", "racecar", false, 4, new Guid("66666666-6666-6666-6666-000000000002") },
                    { new Guid("77777777-7777-7777-7777-000300000001"), "3", "Hello World", true, 1, new Guid("66666666-6666-6666-6666-000000000003") },
                    { new Guid("77777777-7777-7777-7777-000300000002"), "10", "AEIOUaeiou", true, 2, new Guid("66666666-6666-6666-6666-000000000003") },
                    { new Guid("77777777-7777-7777-7777-000300000003"), "0", "xyz", false, 3, new Guid("66666666-6666-6666-6666-000000000003") },
                    { new Guid("77777777-7777-7777-7777-000300000004"), "5", "StudyVerse AI", false, 4, new Guid("66666666-6666-6666-6666-000000000003") },
                    { new Guid("77777777-7777-7777-7777-000400000001"), "15", "1 2 3 4 5", true, 1, new Guid("66666666-6666-6666-6666-000000000004") },
                    { new Guid("77777777-7777-7777-7777-000400000002"), "-6", "-1 -2 -3", true, 2, new Guid("66666666-6666-6666-6666-000000000004") },
                    { new Guid("77777777-7777-7777-7777-000400000003"), "10", "10", false, 3, new Guid("66666666-6666-6666-6666-000000000004") },
                    { new Guid("77777777-7777-7777-7777-000400000004"), "0", "0 0 0 0", false, 4, new Guid("66666666-6666-6666-6666-000000000004") },
                    { new Guid("77777777-7777-7777-7777-000500000001"), "YES", "racecar", true, 1, new Guid("66666666-6666-6666-6666-000000000005") },
                    { new Guid("77777777-7777-7777-7777-000500000002"), "NO", "hello", true, 2, new Guid("66666666-6666-6666-6666-000000000005") },
                    { new Guid("77777777-7777-7777-7777-000500000003"), "YES", "level", false, 3, new Guid("66666666-6666-6666-6666-000000000005") },
                    { new Guid("77777777-7777-7777-7777-000500000004"), "NO", "world", false, 4, new Guid("66666666-6666-6666-6666-000000000005") },
                    { new Guid("77777777-7777-7777-7777-000600000001"), "7", "3 7 5", true, 1, new Guid("66666666-6666-6666-6666-000000000006") },
                    { new Guid("77777777-7777-7777-7777-000600000002"), "-1", "-1 -5 -3", true, 2, new Guid("66666666-6666-6666-6666-000000000006") },
                    { new Guid("77777777-7777-7777-7777-000600000003"), "10", "10 10 5", false, 3, new Guid("66666666-6666-6666-6666-000000000006") },
                    { new Guid("77777777-7777-7777-7777-000600000004"), "0", "0 0 0", false, 4, new Guid("66666666-6666-6666-6666-000000000006") },
                    { new Guid("77777777-7777-7777-7777-000700000001"), "120", "5", true, 1, new Guid("66666666-6666-6666-6666-000000000007") },
                    { new Guid("77777777-7777-7777-7777-000700000002"), "1", "0", true, 2, new Guid("66666666-6666-6666-6666-000000000007") },
                    { new Guid("77777777-7777-7777-7777-000700000003"), "1", "1", false, 3, new Guid("66666666-6666-6666-6666-000000000007") },
                    { new Guid("77777777-7777-7777-7777-000700000004"), "5040", "7", false, 4, new Guid("66666666-6666-6666-6666-000000000007") },
                    { new Guid("77777777-7777-7777-7777-000800000001"), "9", "4 8 2 9 3", true, 1, new Guid("66666666-6666-6666-6666-000000000008") },
                    { new Guid("77777777-7777-7777-7777-000800000002"), "-1", "-5 -1 -10", true, 2, new Guid("66666666-6666-6666-6666-000000000008") },
                    { new Guid("77777777-7777-7777-7777-000800000003"), "7", "7", false, 3, new Guid("66666666-6666-6666-6666-000000000008") },
                    { new Guid("77777777-7777-7777-7777-000800000004"), "5", "1 2 3 4 5", false, 4, new Guid("66666666-6666-6666-6666-000000000008") },
                    { new Guid("77777777-7777-7777-7777-000900000001"), "5", "The quick brown fox jumps", true, 1, new Guid("66666666-6666-6666-6666-000000000009") },
                    { new Guid("77777777-7777-7777-7777-000900000002"), "1", "Hello", true, 2, new Guid("66666666-6666-6666-6666-000000000009") },
                    { new Guid("77777777-7777-7777-7777-000900000003"), "4", "StudyVerse AI is great", false, 3, new Guid("66666666-6666-6666-6666-000000000009") },
                    { new Guid("77777777-7777-7777-7777-000900000004"), "6", "a b c d e f", false, 4, new Guid("66666666-6666-6666-6666-000000000009") },
                    { new Guid("77777777-7777-7777-7777-001000000001"), "1 2 3 4 5 6", "3 3\n1 3 5\n2 4 6", true, 1, new Guid("66666666-6666-6666-6666-000000000010") },
                    { new Guid("77777777-7777-7777-7777-001000000002"), "5 5", "1 1\n5\n5", true, 2, new Guid("66666666-6666-6666-6666-000000000010") },
                    { new Guid("77777777-7777-7777-7777-001000000003"), "0 1 2 3 4 5", "4 2\n1 2 3 4\n0 5", false, 3, new Guid("66666666-6666-6666-6666-000000000010") },
                    { new Guid("77777777-7777-7777-7777-001000000004"), "-3 -2 -1 0 2", "3 2\n-3 -1 2\n-2 0", false, 4, new Guid("66666666-6666-6666-6666-000000000010") },
                    { new Guid("77777777-7777-7777-7777-001100000001"), "YES", "7", true, 1, new Guid("66666666-6666-6666-6666-000000000011") },
                    { new Guid("77777777-7777-7777-7777-001100000002"), "NO", "10", true, 2, new Guid("66666666-6666-6666-6666-000000000011") },
                    { new Guid("77777777-7777-7777-7777-001100000003"), "YES", "2", false, 3, new Guid("66666666-6666-6666-6666-000000000011") },
                    { new Guid("77777777-7777-7777-7777-001100000004"), "NO", "1", false, 4, new Guid("66666666-6666-6666-6666-000000000011") },
                    { new Guid("77777777-7777-7777-7777-001100000005"), "YES", "97", false, 5, new Guid("66666666-6666-6666-6666-000000000011") },
                    { new Guid("77777777-7777-7777-7777-001200000001"), "0 1", "4\n2 7 11 15\n9", true, 1, new Guid("66666666-6666-6666-6666-000000000012") },
                    { new Guid("77777777-7777-7777-7777-001200000002"), "1 2", "3\n3 2 4\n6", true, 2, new Guid("66666666-6666-6666-6666-000000000012") },
                    { new Guid("77777777-7777-7777-7777-001200000003"), "0 1", "2\n3 3\n6", false, 3, new Guid("66666666-6666-6666-6666-000000000012") },
                    { new Guid("77777777-7777-7777-7777-001200000004"), "2 3", "5\n1 5 3 8 2\n11", false, 4, new Guid("66666666-6666-6666-6666-000000000012") },
                    { new Guid("77777777-7777-7777-7777-001300000001"), "YES", "()[]{}", true, 1, new Guid("66666666-6666-6666-6666-000000000013") },
                    { new Guid("77777777-7777-7777-7777-001300000002"), "NO", "(]", true, 2, new Guid("66666666-6666-6666-6666-000000000013") },
                    { new Guid("77777777-7777-7777-7777-001300000003"), "YES", "{[()()]}", false, 3, new Guid("66666666-6666-6666-6666-000000000013") },
                    { new Guid("77777777-7777-7777-7777-001300000004"), "NO", "([)]", false, 4, new Guid("66666666-6666-6666-6666-000000000013") },
                    { new Guid("77777777-7777-7777-7777-001300000005"), "NO", "(((", false, 5, new Guid("66666666-6666-6666-6666-000000000013") },
                    { new Guid("77777777-7777-7777-7777-001400000001"), "3", "5\n1 3 5 7 9\n7", true, 1, new Guid("66666666-6666-6666-6666-000000000014") },
                    { new Guid("77777777-7777-7777-7777-001400000002"), "-1", "5\n1 3 5 7 9\n4", true, 2, new Guid("66666666-6666-6666-6666-000000000014") },
                    { new Guid("77777777-7777-7777-7777-001400000003"), "0", "1\n10\n10", false, 3, new Guid("66666666-6666-6666-6666-000000000014") },
                    { new Guid("77777777-7777-7777-7777-001400000004"), "0", "6\n2 4 6 8 10 12\n2", false, 4, new Guid("66666666-6666-6666-6666-000000000014") },
                    { new Guid("77777777-7777-7777-7777-001400000005"), "5", "6\n2 4 6 8 10 12\n12", false, 5, new Guid("66666666-6666-6666-6666-000000000014") },
                    { new Guid("77777777-7777-7777-7777-001500000001"), "YES", "listen\nsilent", true, 1, new Guid("66666666-6666-6666-6666-000000000015") },
                    { new Guid("77777777-7777-7777-7777-001500000002"), "NO", "hello\nworld", true, 2, new Guid("66666666-6666-6666-6666-000000000015") },
                    { new Guid("77777777-7777-7777-7777-001500000003"), "YES", "Dormitory\nDirty Room", false, 3, new Guid("66666666-6666-6666-6666-000000000015") },
                    { new Guid("77777777-7777-7777-7777-001500000004"), "NO", "abc\nabd", false, 4, new Guid("66666666-6666-6666-6666-000000000015") },
                    { new Guid("77777777-7777-7777-7777-001600000001"), "0", "0", true, 1, new Guid("66666666-6666-6666-6666-000000000016") },
                    { new Guid("77777777-7777-7777-7777-001600000002"), "1", "1", true, 2, new Guid("66666666-6666-6666-6666-000000000016") },
                    { new Guid("77777777-7777-7777-7777-001600000003"), "55", "10", false, 3, new Guid("66666666-6666-6666-6666-000000000016") },
                    { new Guid("77777777-7777-7777-7777-001600000004"), "6765", "20", false, 4, new Guid("66666666-6666-6666-6666-000000000016") },
                    { new Guid("77777777-7777-7777-7777-001700000001"), "6", "-2 1 -3 4 -1 2 1 -5 4", true, 1, new Guid("66666666-6666-6666-6666-000000000017") },
                    { new Guid("77777777-7777-7777-7777-001700000002"), "10", "1 2 3 4", true, 2, new Guid("66666666-6666-6666-6666-000000000017") },
                    { new Guid("77777777-7777-7777-7777-001700000003"), "-1", "-1 -2 -3", false, 3, new Guid("66666666-6666-6666-6666-000000000017") },
                    { new Guid("77777777-7777-7777-7777-001700000004"), "5", "5", false, 4, new Guid("66666666-6666-6666-6666-000000000017") },
                    { new Guid("77777777-7777-7777-7777-001800000001"), "blue is sky The", "The sky is blue", true, 1, new Guid("66666666-6666-6666-6666-000000000018") },
                    { new Guid("77777777-7777-7777-7777-001800000002"), "World Hello", "Hello World", true, 2, new Guid("66666666-6666-6666-6666-000000000018") },
                    { new Guid("77777777-7777-7777-7777-001800000003"), "five four three two one", "one two three four five", false, 3, new Guid("66666666-6666-6666-6666-000000000018") },
                    { new Guid("77777777-7777-7777-7777-001800000004"), "StudyVerse", "StudyVerse", false, 4, new Guid("66666666-6666-6666-6666-000000000018") },
                    { new Guid("77777777-7777-7777-7777-001900000001"), "8", "4 8 2 9 3", true, 1, new Guid("66666666-6666-6666-6666-000000000019") },
                    { new Guid("77777777-7777-7777-7777-001900000002"), "2", "1 1 2 2 3 3", true, 2, new Guid("66666666-6666-6666-6666-000000000019") },
                    { new Guid("77777777-7777-7777-7777-001900000003"), "10", "10 20", false, 3, new Guid("66666666-6666-6666-6666-000000000019") },
                    { new Guid("77777777-7777-7777-7777-001900000004"), "4", "5 5 5 4", false, 4, new Guid("66666666-6666-6666-6666-000000000019") },
                    { new Guid("77777777-7777-7777-7777-002000000001"), "5 4 3 2 1", "1 2 3 4 5", true, 1, new Guid("66666666-6666-6666-6666-000000000020") },
                    { new Guid("77777777-7777-7777-7777-002000000002"), "20 10", "10 20", true, 2, new Guid("66666666-6666-6666-6666-000000000020") },
                    { new Guid("77777777-7777-7777-7777-002000000003"), "7", "7", false, 3, new Guid("66666666-6666-6666-6666-000000000020") },
                    { new Guid("77777777-7777-7777-7777-002000000004"), "8 7 6 5 4 3 2 1", "1 2 3 4 5 6 7 8", false, 4, new Guid("66666666-6666-6666-6666-000000000020") },
                    { new Guid("77777777-7777-7777-7777-002100000001"), "6", "12 18", true, 1, new Guid("66666666-6666-6666-6666-000000000021") },
                    { new Guid("77777777-7777-7777-7777-002100000002"), "1", "17 5", true, 2, new Guid("66666666-6666-6666-6666-000000000021") },
                    { new Guid("77777777-7777-7777-7777-002100000003"), "25", "100 75", false, 3, new Guid("66666666-6666-6666-6666-000000000021") },
                    { new Guid("77777777-7777-7777-7777-002100000004"), "7", "7 7", false, 4, new Guid("66666666-6666-6666-6666-000000000021") },
                    { new Guid("77777777-7777-7777-7777-002200000001"), "bab", "babad", true, 1, new Guid("66666666-6666-6666-6666-000000000022") },
                    { new Guid("77777777-7777-7777-7777-002200000002"), "bb", "cbbd", true, 2, new Guid("66666666-6666-6666-6666-000000000022") },
                    { new Guid("77777777-7777-7777-7777-002200000003"), "a", "a", false, 3, new Guid("66666666-6666-6666-6666-000000000022") },
                    { new Guid("77777777-7777-7777-7777-002200000004"), "geeksskeeg", "forgeeksskeegfor", false, 4, new Guid("66666666-6666-6666-6666-000000000022") },
                    { new Guid("77777777-7777-7777-7777-002300000001"), "3", "abcde\nace", true, 1, new Guid("66666666-6666-6666-6666-000000000023") },
                    { new Guid("77777777-7777-7777-7777-002300000002"), "3", "abc\nabc", true, 2, new Guid("66666666-6666-6666-6666-000000000023") },
                    { new Guid("77777777-7777-7777-7777-002300000003"), "0", "abc\ndef", false, 3, new Guid("66666666-6666-6666-6666-000000000023") },
                    { new Guid("77777777-7777-7777-7777-002300000004"), "5", "abcba\nabcbcba", false, 4, new Guid("66666666-6666-6666-6666-000000000023") },
                    { new Guid("77777777-7777-7777-7777-002400000001"), "7", "5\n7 10 4 3 20\n3", true, 1, new Guid("66666666-6666-6666-6666-000000000024") },
                    { new Guid("77777777-7777-7777-7777-002400000002"), "2", "4\n1 2 2 3\n2", true, 2, new Guid("66666666-6666-6666-6666-000000000024") },
                    { new Guid("77777777-7777-7777-7777-002400000003"), "5", "1\n5\n1", false, 3, new Guid("66666666-6666-6666-6666-000000000024") },
                    { new Guid("77777777-7777-7777-7777-002400000004"), "4", "6\n9 8 7 6 5 4\n1", false, 4, new Guid("66666666-6666-6666-6666-000000000024") },
                    { new Guid("77777777-7777-7777-7777-002500000001"), "3", "5\n2 4 1 3 5", true, 1, new Guid("66666666-6666-6666-6666-000000000025") },
                    { new Guid("77777777-7777-7777-7777-002500000002"), "0", "4\n1 2 3 4", true, 2, new Guid("66666666-6666-6666-6666-000000000025") },
                    { new Guid("77777777-7777-7777-7777-002500000003"), "6", "4\n4 3 2 1", false, 3, new Guid("66666666-6666-6666-6666-000000000025") },
                    { new Guid("77777777-7777-7777-7777-002500000004"), "0", "1\n5", false, 4, new Guid("66666666-6666-6666-6666-000000000025") },
                    { new Guid("77777777-7777-7777-7777-002600000001"), "4", "8\n10 9 2 5 3 7 101 18", true, 1, new Guid("66666666-6666-6666-6666-000000000026") },
                    { new Guid("77777777-7777-7777-7777-002600000002"), "1", "1\n7", true, 2, new Guid("66666666-6666-6666-6666-000000000026") },
                    { new Guid("77777777-7777-7777-7777-002600000003"), "1", "4\n4 3 2 1", false, 3, new Guid("66666666-6666-6666-6666-000000000026") },
                    { new Guid("77777777-7777-7777-7777-002600000004"), "6", "6\n1 2 3 4 5 6", false, 4, new Guid("66666666-6666-6666-6666-000000000026") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_code_submissions_ProblemId",
                table: "code_submissions",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_code_submissions_UserId_ProblemId_Status",
                table: "code_submissions",
                columns: new[] { "UserId", "ProblemId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_code_submissions_UserId_SubmittedAtUtc",
                table: "code_submissions",
                columns: new[] { "UserId", "SubmittedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_coding_problem_test_cases_ProblemId_OrderIndex",
                table: "coding_problem_test_cases",
                columns: new[] { "ProblemId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_coding_problems_Difficulty_Category",
                table: "coding_problems",
                columns: new[] { "Difficulty", "Category" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "code_submissions");

            migrationBuilder.DropTable(
                name: "coding_problem_test_cases");

            migrationBuilder.DropTable(
                name: "coding_problems");
        }
    }
}
