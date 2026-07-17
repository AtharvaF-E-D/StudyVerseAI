using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Dashboard.GetDashboard;

public sealed record GetDashboardQuery(Guid UserId) : IRequest<Result<DashboardDto>>;
