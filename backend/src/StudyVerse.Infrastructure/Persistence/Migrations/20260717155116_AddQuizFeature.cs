using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StudyVerse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quiz_questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Difficulty = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    QuestionText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OptionA = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    OptionB = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    OptionC = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    OptionD = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CorrectOptionIndex = table.Column<int>(type: "integer", nullable: false),
                    Explanation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "quiz_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Difficulty = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Lives = table.Column<int>(type: "integer", nullable: false),
                    CurrentQuestionIndex = table.Column<int>(type: "integer", nullable: false),
                    ComboCount = table.Column<int>(type: "integer", nullable: false),
                    BestComboThisSession = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    XpEarned = table.Column<int>(type: "integer", nullable: false),
                    CoinsEarned = table.Column<int>(type: "integer", nullable: false),
                    UsedFiftyFifty = table.Column<bool>(type: "boolean", nullable: false),
                    UsedExtraTime = table.Column<bool>(type: "boolean", nullable: false),
                    IsDailyChallenge = table.Column<bool>(type: "boolean", nullable: false),
                    DailyChallengeDateUtc = table.Column<DateOnly>(type: "date", nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    EndedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_quiz_sessions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_session_questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    SelectedOptionIndex = table.Column<int>(type: "integer", nullable: true),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: true),
                    TimeTakenMs = table.Column<int>(type: "integer", nullable: true),
                    AnsweredAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_session_questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_quiz_session_questions_quiz_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "quiz_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_quiz_session_questions_quiz_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "quiz_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "quiz_questions",
                columns: new[] { "Id", "Category", "CorrectOptionIndex", "CreatedAtUtc", "Difficulty", "Explanation", "OptionA", "OptionB", "OptionC", "OptionD", "QuestionText" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-001100000001"), "Science", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "Mars appears red because iron oxide (rust) covers much of its surface.", "Venus", "Mars", "Jupiter", "Saturn", "What planet is known as the Red Planet?" },
                    { new Guid("22222222-2222-2222-2222-001100000002"), "Science", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "Plants use carbon dioxide, water, and sunlight to produce glucose and oxygen during photosynthesis.", "Oxygen", "Nitrogen", "Carbon dioxide", "Hydrogen", "What gas do plants absorb from the atmosphere for photosynthesis?" },
                    { new Guid("22222222-2222-2222-2222-001100000003"), "Science", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "Water is made of two hydrogen atoms bonded to one oxygen atom, giving the formula H2O.", "H2O", "CO2", "O2", "NaCl", "What is the chemical formula for water?" },
                    { new Guid("22222222-2222-2222-2222-001100000004"), "Science", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "The adult human skeleton has 206 bones, though babies are born with about 270 that fuse together over time.", "196", "206", "216", "226", "How many bones are in the adult human body?" },
                    { new Guid("22222222-2222-2222-2222-001100000005"), "Science", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "Leaves contain chlorophyll-filled chloroplasts that capture sunlight to drive photosynthesis.", "Root", "Stem", "Leaf", "Flower", "Which part of a plant primarily carries out photosynthesis?" },
                    { new Guid("22222222-2222-2222-2222-001100000006"), "Science", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "Gravity is the force of attraction between Earth's mass and objects near it.", "Magnetism", "Friction", "Gravity", "Tension", "What force pulls objects toward the Earth?" },
                    { new Guid("22222222-2222-2222-2222-001200000001"), "Science", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Mitochondria generate ATP, the cell's main energy currency, through cellular respiration.", "Nucleus", "Ribosome", "Mitochondria", "Golgi apparatus", "What is often called the powerhouse of the cell?" },
                    { new Guid("22222222-2222-2222-2222-001200000002"), "Science", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Hydrogen, with a single proton, is the simplest and lightest element on the periodic table.", "Helium", "Hydrogen", "Oxygen", "Carbon", "Which element has atomic number 1?" },
                    { new Guid("22222222-2222-2222-2222-001200000003"), "Science", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Evaporation is the phase change from liquid to vapor, typically driven by heat.", "Condensation", "Evaporation", "Sublimation", "Precipitation", "What is the process called in which water changes from liquid to gas?" },
                    { new Guid("22222222-2222-2222-2222-001200000004"), "Science", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "White blood cells (leukocytes) are part of the immune system and defend the body against pathogens.", "Red blood cells", "White blood cells", "Platelets", "Stem cells", "Which type of blood cell is primarily responsible for fighting infection?" },
                    { new Guid("22222222-2222-2222-2222-001200000005"), "Science", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Igneous rock forms when molten rock (magma or lava) cools and solidifies.", "Sedimentary", "Metamorphic", "Igneous", "Organic", "What type of rock forms from cooled magma or lava?" },
                    { new Guid("22222222-2222-2222-2222-001200000006"), "Science", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Newton's second law states that force equals mass multiplied by acceleration (F = ma).", "Mass and velocity", "Mass and acceleration", "Weight and distance", "Energy and time", "Newton's second law of motion relates force to which two quantities?" },
                    { new Guid("22222222-2222-2222-2222-001300000001"), "Science", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "Helicase unwinds the double helix by breaking hydrogen bonds between base pairs ahead of the replication fork.", "DNA polymerase", "Helicase", "Ligase", "Primase", "What is the name of the enzyme that unwinds the DNA double helix during replication?" },
                    { new Guid("22222222-2222-2222-2222-001300000002"), "Science", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "Neutrons carry no electric charge, unlike the positively charged proton and negatively charged electron.", "Proton", "Electron", "Neutron", "Positron", "Which subatomic particle carries no electric charge?" },
                    { new Guid("22222222-2222-2222-2222-001300000003"), "Science", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "Electrical resistance is measured in ohms, defined by Ohm's law as voltage divided by current.", "Volt", "Ampere", "Ohm", "Watt", "What is the SI unit of electrical resistance?" },
                    { new Guid("22222222-2222-2222-2222-001300000004"), "Science", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "The law of conservation of energy (the first law of thermodynamics) states that the total energy of an isolated system remains constant.", "Newton's first law", "Law of conservation of energy", "Boyle's law", "Law of conservation of momentum", "Which law states that energy cannot be created or destroyed, only converted from one form to another?" },
                    { new Guid("22222222-2222-2222-2222-001300000005"), "Science", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "The genome is the entirety of an organism's hereditary genetic information encoded in its DNA.", "Phenotype", "Genome", "Chromosome", "Allele", "What is the term for an organism's complete set of genetic material?" },
                    { new Guid("22222222-2222-2222-2222-001300000006"), "Science", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "Photons are the force-carrying particles (bosons) of the electromagnetic interaction, per quantum electrodynamics.", "Gluon", "Photon", "W boson", "Graviton", "Which particle is exchanged to mediate the electromagnetic force?" },
                    { new Guid("22222222-2222-2222-2222-002100000001"), "Mathematics", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "7 multiplied by 8 equals 56.", "54", "56", "58", "64", "What is 7 x 8?" },
                    { new Guid("22222222-2222-2222-2222-002100000002"), "Mathematics", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "Pi, the ratio of a circle's circumference to its diameter, is approximately 3.14159, which rounds to 3.14.", "3.14", "3.41", "3.12", "3.16", "What is the value of pi rounded to two decimal places?" },
                    { new Guid("22222222-2222-2222-2222-002100000003"), "Mathematics", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "A hexagon is a polygon with six sides and six angles.", "5", "6", "7", "8", "How many sides does a hexagon have?" },
                    { new Guid("22222222-2222-2222-2222-002100000004"), "Mathematics", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "15% of 200 is calculated as 0.15 x 200 = 30.", "20", "25", "30", "35", "What is 15% of 200?" },
                    { new Guid("22222222-2222-2222-2222-002100000005"), "Mathematics", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "A prime number has exactly two positive divisors: 1 and itself.", "Composite number", "Prime number", "Even number", "Whole number", "What do you call a number greater than 1 that can only be divided evenly by 1 and itself?" },
                    { new Guid("22222222-2222-2222-2222-002100000006"), "Mathematics", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "9 multiplied by itself (9 x 9) equals 81, so the square root of 81 is 9.", "7", "8", "9", "10", "What is the square root of 81?" },
                    { new Guid("22222222-2222-2222-2222-002200000001"), "Mathematics", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Subtracting 5 from both sides gives 2x = 12, and dividing by 2 gives x = 6.", "5", "6", "7", "8", "What is the value of x in the equation 2x + 5 = 17?" },
                    { new Guid("22222222-2222-2222-2222-002200000002"), "Mathematics", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "The area of a circle equals pi times the radius squared: 3.14 x 4^2 = 3.14 x 16 = 50.24.", "25.12", "50.24", "12.56", "100.48", "What is the area of a circle with radius 4 (using pi ≈ 3.14)?" },
                    { new Guid("22222222-2222-2222-2222-002200000003"), "Mathematics", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "The Pythagorean theorem states a^2 + b^2 = c^2 for a right triangle, where c is the hypotenuse.", "Euclid's theorem", "Pythagorean theorem", "Fermat's theorem", "Thales' theorem", "In a right triangle, what is the theorem relating the lengths of the three sides called?" },
                    { new Guid("22222222-2222-2222-2222-002200000004"), "Mathematics", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "The interior angles of any triangle always sum to 180 degrees.", "90 degrees", "180 degrees", "270 degrees", "360 degrees", "What is the sum of the interior angles of a triangle?" },
                    { new Guid("22222222-2222-2222-2222-002200000005"), "Mathematics", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "3! means 3 x 2 x 1, which equals 6.", "3", "6", "9", "12", "What is 3 factorial (3!)?" },
                    { new Guid("22222222-2222-2222-2222-002200000006"), "Mathematics", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Parallel lines lie in the same plane and never meet, maintaining a constant distance apart.", "Perpendicular", "Parallel", "Intersecting", "Skew", "What term describes two lines in the same plane that never intersect?" },
                    { new Guid("22222222-2222-2222-2222-002300000001"), "Mathematics", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "Using the power rule, the derivative of x^2 is 2x^(2-1) = 2x.", "x", "2x", "x^2", "2", "What is the derivative of x^2 with respect to x?" },
                    { new Guid("22222222-2222-2222-2222-002300000002"), "Mathematics", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "Euler's number e, the base of natural logarithms, is approximately 2.71828, which rounds to 2.71.", "2.71", "2.61", "3.14", "1.61", "What is the value of the mathematical constant e, rounded to two decimal places?" },
                    { new Guid("22222222-2222-2222-2222-002300000003"), "Mathematics", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "The determinant of an identity matrix, of any size, is always 1.", "0", "1", "2", "-1", "What is the determinant of a 2x2 identity matrix?" },
                    { new Guid("22222222-2222-2222-2222-002300000004"), "Mathematics", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "Standard deviation quantifies how much data values deviate from the mean, measuring dispersion.", "Central tendency", "Data spread/dispersion", "Correlation", "Probability", "In statistics, what does the standard deviation measure?" },
                    { new Guid("22222222-2222-2222-2222-002300000005"), "Mathematics", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "Using the formula n(n+1)/2 with n=10 gives 10 x 11 / 2 = 55.", "45", "50", "55", "60", "What is the sum of the first 10 positive integers?" },
                    { new Guid("22222222-2222-2222-2222-002300000006"), "Mathematics", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "Irrational numbers have non-terminating, non-repeating decimal expansions and cannot be written as a ratio of two integers.", "Rational number", "Irrational number", "Complex number", "Natural number", "What kind of number, such as pi or the square root of 2, cannot be written as a simple fraction?" },
                    { new Guid("22222222-2222-2222-2222-003100000001"), "History", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "World War II ended in 1945, with Germany surrendering in May and Japan in September.", "1943", "1945", "1947", "1950", "In what year did World War II end?" },
                    { new Guid("22222222-2222-2222-2222-003100000002"), "History", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "George Washington served as the first U.S. President from 1789 to 1797.", "Thomas Jefferson", "John Adams", "George Washington", "Abraham Lincoln", "Who was the first President of the United States?" },
                    { new Guid("22222222-2222-2222-2222-003100000003"), "History", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "The ancient Egyptians built the Giza pyramids as tombs for their pharaohs around 2600-2500 BCE.", "Romans", "Greeks", "Egyptians", "Mesopotamians", "Which ancient civilization built the pyramids of Giza?" },
                    { new Guid("22222222-2222-2222-2222-003100000004"), "History", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "The Berlin Wall divided East and West Berlin from 1961 until it fell in 1989.", "The Great Wall", "The Berlin Wall", "Hadrian's Wall", "The Atlantic Wall", "Which wall divided a European city for almost 30 years during the Cold War?" },
                    { new Guid("22222222-2222-2222-2222-003100000005"), "History", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "Christopher Columbus's 1492 voyage, sponsored by Spain, is traditionally credited with opening sustained European contact with the Americas.", "Vasco da Gama", "Christopher Columbus", "Ferdinand Magellan", "Marco Polo", "Who completed the famous 1492 transatlantic voyage credited with opening European contact with the Americas?" },
                    { new Guid("22222222-2222-2222-2222-003100000006"), "History", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "The Colosseum in Rome hosted gladiatorial combat and public spectacles beginning around 80 CE.", "The Parthenon", "The Colosseum", "The Pantheon", "The Acropolis", "Which ancient Roman structure was used for gladiator contests?" },
                    { new Guid("22222222-2222-2222-2222-003200000001"), "History", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Julius Caesar was a Roman general and statesman who became dictator of the Roman Republic, paving the way for the Roman Empire that followed under Augustus.", "Ottoman", "Roman", "Byzantine", "Persian", "Julius Caesar was a general and statesman of which ancient civilization?" },
                    { new Guid("22222222-2222-2222-2222-003200000002"), "History", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "King John of England signed the Magna Carta in 1215, limiting royal power and establishing lasting legal principles.", "1066", "1215", "1348", "1492", "The Magna Carta was signed in what year?" },
                    { new Guid("22222222-2222-2222-2222-003200000003"), "History", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "The Industrial Revolution began in Great Britain in the late 18th century, driven by innovations like the steam engine.", "France", "Germany", "United Kingdom", "United States", "Which country was the first to industrialize during the Industrial Revolution?" },
                    { new Guid("22222222-2222-2222-2222-003200000004"), "History", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Joseph Stalin led the Soviet Union throughout World War II, from its start in 1939 through the Allied victory in 1945.", "Vladimir Lenin", "Joseph Stalin", "Nikita Khrushchev", "Leon Trotsky", "Who led the Soviet Union throughout most of World War II?" },
                    { new Guid("22222222-2222-2222-2222-003200000005"), "History", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "The French Revolution began in 1789 with the storming of the Bastille and the collapse of the monarchy's absolute power.", "1776", "1789", "1804", "1815", "The French Revolution began in what year?" },
                    { new Guid("22222222-2222-2222-2222-003200000006"), "History", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Ferdinand Magellan's expedition, completed after his death by his surviving crew, was the first to circumnavigate the Earth, from 1519-1522.", "Christopher Columbus", "Ferdinand Magellan", "James Cook", "Vasco da Gama", "Which explorer's expedition was the first to circumnavigate the globe?" },
                    { new Guid("22222222-2222-2222-2222-003300000001"), "History", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "The Peace of Westphalia established the principle of state sovereignty, ending the Thirty Years' War and shaping the modern nation-state system.", "Democracy", "State sovereignty", "Free trade", "Human rights", "The Peace of Westphalia (1648) is most significant for establishing which concept in international relations?" },
                    { new Guid("22222222-2222-2222-2222-003300000002"), "History", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "Qin Shi Huang unified the warring states of China under the Qin Dynasty in 221 BCE, becoming its first emperor.", "Han Dynasty", "Qin Dynasty", "Tang Dynasty", "Ming Dynasty", "Which dynasty first unified China under Qin Shi Huang?" },
                    { new Guid("22222222-2222-2222-2222-003300000003"), "History", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "The assassination of Archduke Franz Ferdinand of Austria-Hungary in Sarajevo in June 1914 set off the alliance system that led to World War I.", "The bombing of Pearl Harbor", "The assassination of Archduke Franz Ferdinand", "The invasion of Poland", "The sinking of the Lusitania", "Which event directly triggered the start of World War I?" },
                    { new Guid("22222222-2222-2222-2222-003300000004"), "History", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "The Congress of Vienna reorganized Europe's borders and balance of power following the Napoleonic Wars.", "End slavery", "Redraw Europe's political map after Napoleon", "Establish the League of Nations", "Divide Africa among colonial powers", "The Congress of Vienna (1814-1815) was convened primarily to do what?" },
                    { new Guid("22222222-2222-2222-2222-003300000005"), "History", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "Hammurabi, king of Babylon, issued the Code of Hammurabi around 1754 BCE, one of the earliest and most complete written legal codes.", "Sargon of Akkad", "Hammurabi", "Nebuchadnezzar", "Cyrus the Great", "Which ancient Mesopotamian king is known for one of the earliest surviving written law codes?" },
                    { new Guid("22222222-2222-2222-2222-003300000006"), "History", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "The Meiji Restoration restored imperial rule in Japan and launched an era of rapid modernization and industrialization.", "China", "Korea", "Japan", "Vietnam", "The Meiji Restoration of 1868 marked a turning point in the history of which country?" },
                    { new Guid("22222222-2222-2222-2222-004100000001"), "Geography", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "Asia is the largest continent, covering about 30% of Earth's total land area.", "Africa", "Asia", "Europe", "North America", "What is the largest continent by area?" },
                    { new Guid("22222222-2222-2222-2222-004100000002"), "Geography", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "The Nile River, flowing through northeastern Africa, is traditionally considered the longest river at about 6,650 km.", "Amazon River", "Nile River", "Yangtze River", "Mississippi River", "Which river is traditionally considered the longest in the world?" },
                    { new Guid("22222222-2222-2222-2222-004100000003"), "Geography", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "Paris has been the capital of France since the late 10th century.", "Lyon", "Marseille", "Paris", "Nice", "What is the capital city of France?" },
                    { new Guid("22222222-2222-2222-2222-004100000004"), "Geography", 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "The Pacific Ocean is the largest and deepest ocean, covering more area than all of Earth's landmass combined.", "Atlantic Ocean", "Indian Ocean", "Arctic Ocean", "Pacific Ocean", "Which ocean is the largest in the world?" },
                    { new Guid("22222222-2222-2222-2222-004100000005"), "Geography", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "Vatican City, an independent city-state in Rome, covers about 0.44 square kilometers, making it the world's smallest country.", "Monaco", "Vatican City", "San Marino", "Liechtenstein", "What is the smallest country in the world by area?" },
                    { new Guid("22222222-2222-2222-2222-004100000006"), "Geography", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "Mount Everest, Earth's highest peak, sits in the Himalayas on the border of Nepal and Tibet.", "Andes", "Alps", "Himalayas", "Rockies", "Mount Everest is located in which mountain range?" },
                    { new Guid("22222222-2222-2222-2222-004200000001"), "Geography", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "The Sahara Desert in North Africa is the largest hot desert, covering roughly 9.2 million square kilometers.", "Gobi Desert", "Kalahari Desert", "Sahara Desert", "Sonoran Desert", "Which desert is the largest hot desert in the world?" },
                    { new Guid("22222222-2222-2222-2222-004200000002"), "Geography", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Canada contains more lakes than the rest of the world's countries combined, a legacy of glacial activity during the last ice age.", "United States", "Russia", "Canada", "Finland", "Which country contains more natural lakes than any other?" },
                    { new Guid("22222222-2222-2222-2222-004200000003"), "Geography", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "The Prime Meridian, passing through Greenwich, England, marks 0 degrees longitude and is the reference point for world time zones.", "Equator", "Tropic of Cancer", "Prime Meridian", "International Date Line", "What is the name of the imaginary line at 0 degrees longitude?" },
                    { new Guid("22222222-2222-2222-2222-004200000004"), "Geography", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Ethiopia was historically known as Abyssinia and is one of the oldest continuously independent countries in Africa.", "Kenya", "Ethiopia", "Sudan", "Somalia", "Which African country was formerly known as Abyssinia?" },
                    { new Guid("22222222-2222-2222-2222-004200000005"), "Geography", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "The Strait of Gibraltar separates Spain (Europe) from Morocco (Africa), narrowing to about 14 km wide.", "Strait of Hormuz", "Strait of Gibraltar", "Bosphorus Strait", "Bering Strait", "Which strait separates Europe and Africa at its narrowest point?" },
                    { new Guid("22222222-2222-2222-2222-004200000006"), "Geography", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Antarctica is technically a polar desert, receiving very little precipitation despite its ice cover, making it the driest continent.", "Africa", "Australia", "Antarctica", "Asia", "Which continent is technically the driest on Earth?" },
                    { new Guid("22222222-2222-2222-2222-004300000001"), "Geography", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "France has 12 time zones because of its overseas territories scattered across the globe, more than any other country.", "Russia", "United States", "France", "China", "Which country has the most time zones, including its overseas territories?" },
                    { new Guid("22222222-2222-2222-2222-004300000002"), "Geography", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "The Ring of Fire traces tectonic plate boundaries around the Pacific Ocean, producing intense seismic and volcanic activity.", "The Great Rift Valley", "The Ring of Fire", "The Mid-Atlantic Ridge", "The San Andreas Fault", "What is the name of the horseshoe-shaped zone around the Pacific Ocean known for frequent earthquakes and volcanic activity?" },
                    { new Guid("22222222-2222-2222-2222-004300000003"), "Geography", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "The Caspian Sea, bordered by five countries, is the largest enclosed inland body of water on Earth by area.", "Dead Sea", "Caspian Sea", "Aral Sea", "Black Sea", "Which landlocked body of water is actually the largest enclosed inland body of water on Earth?" },
                    { new Guid("22222222-2222-2222-2222-004300000004"), "Geography", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "The Coriolis effect arises from Earth's rotation, deflecting moving air and water to the right in the Northern Hemisphere and left in the Southern Hemisphere.", "Earth's magnetic field", "Earth's rotation", "The moon's gravity", "Solar radiation", "What primarily causes the Coriolis effect that influences wind and ocean current directions?" },
                    { new Guid("22222222-2222-2222-2222-004300000005"), "Geography", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "Turkey spans two continents, with Istanbul straddling the Bosphorus strait that divides Europe and Asia.", "Russia", "Kazakhstan", "Turkey", "Georgia", "Which country straddles both Europe and Asia across the Bosphorus?" },
                    { new Guid("22222222-2222-2222-2222-004300000006"), "Geography", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "An island is a piece of land completely surrounded by water and smaller in scale than a continent.", "Peninsula", "Archipelago", "Island", "Isthmus", "What is the term for a landmass entirely surrounded by water but smaller in scale than a continent?" },
                    { new Guid("22222222-2222-2222-2222-005100000001"), "General Knowledge", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "Earth has seven continents: Africa, Antarctica, Asia, Australia, Europe, North America, and South America.", "5", "6", "7", "8", "How many continents are there on Earth?" },
                    { new Guid("22222222-2222-2222-2222-005100000002"), "General Knowledge", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "Mixing blue and yellow pigments produces green.", "Purple", "Orange", "Green", "Brown", "What color do you get by mixing blue and yellow paint?" },
                    { new Guid("22222222-2222-2222-2222-005100000003"), "General Knowledge", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "A leap year has 366 days, with an extra day (February 29) added to keep the calendar aligned with Earth's orbit.", "364", "365", "366", "367", "How many days are there in a leap year?" },
                    { new Guid("22222222-2222-2222-2222-005100000004"), "General Knowledge", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "The blue whale is the largest animal known to have ever existed, reaching up to 30 meters in length.", "African elephant", "Blue whale", "Giraffe", "Polar bear", "What is the largest mammal in the world?" },
                    { new Guid("22222222-2222-2222-2222-005100000005"), "General Knowledge", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "The piano produces sound when pressed keys cause hammers to strike strings inside the instrument.", "Violin", "Piano", "Flute", "Drum", "Which instrument has keys, pedals, and strings, and is played by pressing its keys?" },
                    { new Guid("22222222-2222-2222-2222-005100000006"), "General Knowledge", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Easy", "Bees produce honey from flower nectar, storing it as food inside their hive.", "Milk", "Honey", "Silk", "Butter", "What sweet food do bees produce from flower nectar?" },
                    { new Guid("22222222-2222-2222-2222-005200000001"), "General Knowledge", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "William Shakespeare wrote \"Romeo and Juliet\" around 1594-1596, one of his most famous tragedies.", "Charles Dickens", "William Shakespeare", "Mark Twain", "Jane Austen", "Who wrote the play \"Romeo and Juliet\"?" },
                    { new Guid("22222222-2222-2222-2222-005200000002"), "General Knowledge", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "The Japanese yen is the official currency of Japan.", "Won", "Yuan", "Yen", "Ringgit", "What is the national currency of Japan?" },
                    { new Guid("22222222-2222-2222-2222-005200000003"), "General Knowledge", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Saturn's rings, made mostly of ice and rock particles, are the most extensive and visible ring system in the solar system.", "Mars", "Saturn", "Neptune", "Mercury", "Which planet is best known for its prominent, easily visible ring system?" },
                    { new Guid("22222222-2222-2222-2222-005200000004"), "General Knowledge", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Zeus is the king of the Olympian gods in Greek mythology, ruling over the sky and thunder.", "Poseidon", "Hades", "Zeus", "Apollo", "In Greek mythology, who is the king of the gods?" },
                    { new Guid("22222222-2222-2222-2222-005200000005"), "General Knowledge", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "Guacamole is primarily made from mashed avocado, often mixed with lime, onion, and seasonings.", "Tomato", "Avocado", "Onion", "Lime", "What is the primary ingredient in traditional guacamole?" },
                    { new Guid("22222222-2222-2222-2222-005200000006"), "General Knowledge", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medium", "In tennis, \"love\" is the traditional term used for a score of zero.", "Basketball", "Tennis", "Cricket", "Golf", "In which sport is the term \"love\" used to mean a score of zero?" },
                    { new Guid("22222222-2222-2222-2222-005300000001"), "General Knowledge", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "Mycology is the branch of biology dedicated to the study of fungi.", "Botany", "Mycology", "Entomology", "Herpetology", "What is the scientific study of fungi called?" },
                    { new Guid("22222222-2222-2222-2222-005300000002"), "General Knowledge", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "Plato authored \"The Republic\", a foundational work of Western philosophy examining justice and the ideal society through dialogues.", "Aristotle", "Socrates", "Plato", "Epicurus", "Which philosopher wrote \"The Republic\", exploring justice and the ideal state?" },
                    { new Guid("22222222-2222-2222-2222-005300000003"), "General Knowledge", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "A palindrome reads the same forward and backward, like \"level\" or \"racecar\".", "Anagram", "Palindrome", "Acronym", "Homograph", "What is the term for a word that reads the same forwards and backwards?" },
                    { new Guid("22222222-2222-2222-2222-005300000004"), "General Knowledge", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "The Nobel Foundation, based in Sweden, manages the assets and administers the awarding of the Nobel Prizes established by Alfred Nobel's will.", "United Nations", "The Nobel Foundation", "UNESCO", "The Red Cross", "Which organization administers the awarding of the Nobel Prizes?" },
                    { new Guid("22222222-2222-2222-2222-005300000005"), "General Knowledge", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "A group of crows is traditionally called \"a murder\", a term rooted in old folklore about the birds.", "A flock", "A murder", "A parliament", "A pod", "What is a group of crows traditionally called?" },
                    { new Guid("22222222-2222-2222-2222-005300000006"), "General Knowledge", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hard", "Michelangelo painted the Sistine Chapel ceiling between 1508 and 1512, including the iconic \"Creation of Adam\".", "Leonardo da Vinci", "Raphael", "Michelangelo", "Donatello", "Which artist painted the ceiling of the Sistine Chapel?" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_quiz_questions_Category_Difficulty",
                table: "quiz_questions",
                columns: new[] { "Category", "Difficulty" });

            migrationBuilder.CreateIndex(
                name: "IX_quiz_session_questions_QuestionId",
                table: "quiz_session_questions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_session_questions_SessionId_OrderIndex",
                table: "quiz_session_questions",
                columns: new[] { "SessionId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quiz_sessions_UserId_DailyChallengeDateUtc",
                table: "quiz_sessions",
                columns: new[] { "UserId", "DailyChallengeDateUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quiz_sessions_UserId_StartedAtUtc",
                table: "quiz_sessions",
                columns: new[] { "UserId", "StartedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quiz_session_questions");

            migrationBuilder.DropTable(
                name: "quiz_questions");

            migrationBuilder.DropTable(
                name: "quiz_sessions");
        }
    }
}
