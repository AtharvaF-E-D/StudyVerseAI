using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Features.Quiz.Common;

internal static class QuizMapping
{
    public static string[] GetOptions(QuizQuestion question) =>
        [question.OptionA, question.OptionB, question.OptionC, question.OptionD];

    public static QuizQuestionOptionsDto ToOptionsDto(QuizQuestion question) =>
        new(question.Id, question.QuestionText, GetOptions(question));
}
