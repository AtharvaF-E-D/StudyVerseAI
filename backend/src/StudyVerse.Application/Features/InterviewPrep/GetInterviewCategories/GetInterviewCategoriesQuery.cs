using MediatR;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.InterviewPrep.GetInterviewCategories;

public sealed record GetInterviewCategoriesQuery() : IRequest<Result<IReadOnlyList<InterviewCategoryDto>>>;
