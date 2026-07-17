using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Quiz;

namespace StudyVerse.Infrastructure.Persistence.Seed;

/// <summary>
/// The static Rapid Fire Quiz question bank, applied via <c>QuizQuestionConfiguration.HasData</c>.
/// Ids are stable hardcoded GUIDs (pattern: <c>22222222-2222-2222-2222-CCCDNNNNNNNN</c>, where
/// <c>CCC</c> is a 3-digit category code, <c>D</c> is a 1-digit difficulty code, and
/// <c>NNNNNNNN</c> is an 8-digit per-category-per-difficulty sequence number) so the seed stays
/// idempotent across migrations — the same reasoning as the Phase 3 <c>ChallengeTemplate</c>
/// static ids. 90 real, non-trivial trivia questions: 5 categories × 3 difficulties × 6 questions
/// each, comfortably clearing the ≥75 minimum with roughly even distribution.
/// </summary>
public static class QuizQuestionSeedData
{
    // Fixed (not DateTime.UtcNow) because EF Core HasData values must be static/deterministic —
    // a changing CreatedAtUtc would produce a spurious migration diff on every `migrations add`.
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // Category codes used in the Id scheme below.
    private const string Sci = "001";
    private const string Math = "002";
    private const string Hist = "003";
    private const string Geo = "004";
    private const string Gk = "005";

    public static IReadOnlyList<QuizQuestion> All { get; } = BuildAll();

    private static QuizQuestion Q(
        string categoryCode,
        int difficultyCode,
        int sequence,
        string category,
        QuizDifficulty difficulty,
        string text,
        string optionA,
        string optionB,
        string optionC,
        string optionD,
        int correctOptionIndex,
        string explanation) => new()
        {
            Id = Guid.Parse($"22222222-2222-2222-2222-{categoryCode}{difficultyCode}{sequence:D8}"),
            Category = category,
            Difficulty = difficulty,
            QuestionText = text,
            OptionA = optionA,
            OptionB = optionB,
            OptionC = optionC,
            OptionD = optionD,
            CorrectOptionIndex = correctOptionIndex,
            Explanation = explanation,
            CreatedAtUtc = SeedTimestamp,
        };

    private static List<QuizQuestion> BuildAll()
    {
        var list = new List<QuizQuestion>();

        // ---------------- Science ----------------
        const string cat = QuizCategories.Science;

        list.Add(Q(Sci, 1, 1, cat, QuizDifficulty.Easy,
            "What planet is known as the Red Planet?",
            "Venus", "Mars", "Jupiter", "Saturn", 1,
            "Mars appears red because iron oxide (rust) covers much of its surface."));
        list.Add(Q(Sci, 1, 2, cat, QuizDifficulty.Easy,
            "What gas do plants absorb from the atmosphere for photosynthesis?",
            "Oxygen", "Nitrogen", "Carbon dioxide", "Hydrogen", 2,
            "Plants use carbon dioxide, water, and sunlight to produce glucose and oxygen during photosynthesis."));
        list.Add(Q(Sci, 1, 3, cat, QuizDifficulty.Easy,
            "What is the chemical formula for water?",
            "H2O", "CO2", "O2", "NaCl", 0,
            "Water is made of two hydrogen atoms bonded to one oxygen atom, giving the formula H2O."));
        list.Add(Q(Sci, 1, 4, cat, QuizDifficulty.Easy,
            "How many bones are in the adult human body?",
            "196", "206", "216", "226", 1,
            "The adult human skeleton has 206 bones, though babies are born with about 270 that fuse together over time."));
        list.Add(Q(Sci, 1, 5, cat, QuizDifficulty.Easy,
            "Which part of a plant primarily carries out photosynthesis?",
            "Root", "Stem", "Leaf", "Flower", 2,
            "Leaves contain chlorophyll-filled chloroplasts that capture sunlight to drive photosynthesis."));
        list.Add(Q(Sci, 1, 6, cat, QuizDifficulty.Easy,
            "What force pulls objects toward the Earth?",
            "Magnetism", "Friction", "Gravity", "Tension", 2,
            "Gravity is the force of attraction between Earth's mass and objects near it."));

        list.Add(Q(Sci, 2, 1, cat, QuizDifficulty.Medium,
            "What is often called the powerhouse of the cell?",
            "Nucleus", "Ribosome", "Mitochondria", "Golgi apparatus", 2,
            "Mitochondria generate ATP, the cell's main energy currency, through cellular respiration."));
        list.Add(Q(Sci, 2, 2, cat, QuizDifficulty.Medium,
            "Which element has atomic number 1?",
            "Helium", "Hydrogen", "Oxygen", "Carbon", 1,
            "Hydrogen, with a single proton, is the simplest and lightest element on the periodic table."));
        list.Add(Q(Sci, 2, 3, cat, QuizDifficulty.Medium,
            "What is the process called in which water changes from liquid to gas?",
            "Condensation", "Evaporation", "Sublimation", "Precipitation", 1,
            "Evaporation is the phase change from liquid to vapor, typically driven by heat."));
        list.Add(Q(Sci, 2, 4, cat, QuizDifficulty.Medium,
            "Which type of blood cell is primarily responsible for fighting infection?",
            "Red blood cells", "White blood cells", "Platelets", "Stem cells", 1,
            "White blood cells (leukocytes) are part of the immune system and defend the body against pathogens."));
        list.Add(Q(Sci, 2, 5, cat, QuizDifficulty.Medium,
            "What type of rock forms from cooled magma or lava?",
            "Sedimentary", "Metamorphic", "Igneous", "Organic", 2,
            "Igneous rock forms when molten rock (magma or lava) cools and solidifies."));
        list.Add(Q(Sci, 2, 6, cat, QuizDifficulty.Medium,
            "Newton's second law of motion relates force to which two quantities?",
            "Mass and velocity", "Mass and acceleration", "Weight and distance", "Energy and time", 1,
            "Newton's second law states that force equals mass multiplied by acceleration (F = ma)."));

        list.Add(Q(Sci, 3, 1, cat, QuizDifficulty.Hard,
            "What is the name of the enzyme that unwinds the DNA double helix during replication?",
            "DNA polymerase", "Helicase", "Ligase", "Primase", 1,
            "Helicase unwinds the double helix by breaking hydrogen bonds between base pairs ahead of the replication fork."));
        list.Add(Q(Sci, 3, 2, cat, QuizDifficulty.Hard,
            "Which subatomic particle carries no electric charge?",
            "Proton", "Electron", "Neutron", "Positron", 2,
            "Neutrons carry no electric charge, unlike the positively charged proton and negatively charged electron."));
        list.Add(Q(Sci, 3, 3, cat, QuizDifficulty.Hard,
            "What is the SI unit of electrical resistance?",
            "Volt", "Ampere", "Ohm", "Watt", 2,
            "Electrical resistance is measured in ohms, defined by Ohm's law as voltage divided by current."));
        list.Add(Q(Sci, 3, 4, cat, QuizDifficulty.Hard,
            "Which law states that energy cannot be created or destroyed, only converted from one form to another?",
            "Newton's first law", "Law of conservation of energy", "Boyle's law", "Law of conservation of momentum", 1,
            "The law of conservation of energy (the first law of thermodynamics) states that the total energy of an isolated system remains constant."));
        list.Add(Q(Sci, 3, 5, cat, QuizDifficulty.Hard,
            "What is the term for an organism's complete set of genetic material?",
            "Phenotype", "Genome", "Chromosome", "Allele", 1,
            "The genome is the entirety of an organism's hereditary genetic information encoded in its DNA."));
        list.Add(Q(Sci, 3, 6, cat, QuizDifficulty.Hard,
            "Which particle is exchanged to mediate the electromagnetic force?",
            "Gluon", "Photon", "W boson", "Graviton", 1,
            "Photons are the force-carrying particles (bosons) of the electromagnetic interaction, per quantum electrodynamics."));

        // ---------------- Mathematics ----------------
        const string catMath = QuizCategories.Mathematics;

        list.Add(Q(Math, 1, 1, catMath, QuizDifficulty.Easy,
            "What is 7 x 8?",
            "54", "56", "58", "64", 1,
            "7 multiplied by 8 equals 56."));
        list.Add(Q(Math, 1, 2, catMath, QuizDifficulty.Easy,
            "What is the value of pi rounded to two decimal places?",
            "3.14", "3.41", "3.12", "3.16", 0,
            "Pi, the ratio of a circle's circumference to its diameter, is approximately 3.14159, which rounds to 3.14."));
        list.Add(Q(Math, 1, 3, catMath, QuizDifficulty.Easy,
            "How many sides does a hexagon have?",
            "5", "6", "7", "8", 1,
            "A hexagon is a polygon with six sides and six angles."));
        list.Add(Q(Math, 1, 4, catMath, QuizDifficulty.Easy,
            "What is 15% of 200?",
            "20", "25", "30", "35", 2,
            "15% of 200 is calculated as 0.15 x 200 = 30."));
        list.Add(Q(Math, 1, 5, catMath, QuizDifficulty.Easy,
            "What do you call a number greater than 1 that can only be divided evenly by 1 and itself?",
            "Composite number", "Prime number", "Even number", "Whole number", 1,
            "A prime number has exactly two positive divisors: 1 and itself."));
        list.Add(Q(Math, 1, 6, catMath, QuizDifficulty.Easy,
            "What is the square root of 81?",
            "7", "8", "9", "10", 2,
            "9 multiplied by itself (9 x 9) equals 81, so the square root of 81 is 9."));

        list.Add(Q(Math, 2, 1, catMath, QuizDifficulty.Medium,
            "What is the value of x in the equation 2x + 5 = 17?",
            "5", "6", "7", "8", 1,
            "Subtracting 5 from both sides gives 2x = 12, and dividing by 2 gives x = 6."));
        list.Add(Q(Math, 2, 2, catMath, QuizDifficulty.Medium,
            "What is the area of a circle with radius 4 (using pi ≈ 3.14)?",
            "25.12", "50.24", "12.56", "100.48", 1,
            "The area of a circle equals pi times the radius squared: 3.14 x 4^2 = 3.14 x 16 = 50.24."));
        list.Add(Q(Math, 2, 3, catMath, QuizDifficulty.Medium,
            "In a right triangle, what is the theorem relating the lengths of the three sides called?",
            "Euclid's theorem", "Pythagorean theorem", "Fermat's theorem", "Thales' theorem", 1,
            "The Pythagorean theorem states a^2 + b^2 = c^2 for a right triangle, where c is the hypotenuse."));
        list.Add(Q(Math, 2, 4, catMath, QuizDifficulty.Medium,
            "What is the sum of the interior angles of a triangle?",
            "90 degrees", "180 degrees", "270 degrees", "360 degrees", 1,
            "The interior angles of any triangle always sum to 180 degrees."));
        list.Add(Q(Math, 2, 5, catMath, QuizDifficulty.Medium,
            "What is 3 factorial (3!)?",
            "3", "6", "9", "12", 1,
            "3! means 3 x 2 x 1, which equals 6."));
        list.Add(Q(Math, 2, 6, catMath, QuizDifficulty.Medium,
            "What term describes two lines in the same plane that never intersect?",
            "Perpendicular", "Parallel", "Intersecting", "Skew", 1,
            "Parallel lines lie in the same plane and never meet, maintaining a constant distance apart."));

        list.Add(Q(Math, 3, 1, catMath, QuizDifficulty.Hard,
            "What is the derivative of x^2 with respect to x?",
            "x", "2x", "x^2", "2", 1,
            "Using the power rule, the derivative of x^2 is 2x^(2-1) = 2x."));
        list.Add(Q(Math, 3, 2, catMath, QuizDifficulty.Hard,
            "What is the value of the mathematical constant e, rounded to two decimal places?",
            "2.71", "2.61", "3.14", "1.61", 0,
            "Euler's number e, the base of natural logarithms, is approximately 2.71828, which rounds to 2.71."));
        list.Add(Q(Math, 3, 3, catMath, QuizDifficulty.Hard,
            "What is the determinant of a 2x2 identity matrix?",
            "0", "1", "2", "-1", 1,
            "The determinant of an identity matrix, of any size, is always 1."));
        list.Add(Q(Math, 3, 4, catMath, QuizDifficulty.Hard,
            "In statistics, what does the standard deviation measure?",
            "Central tendency", "Data spread/dispersion", "Correlation", "Probability", 1,
            "Standard deviation quantifies how much data values deviate from the mean, measuring dispersion."));
        list.Add(Q(Math, 3, 5, catMath, QuizDifficulty.Hard,
            "What is the sum of the first 10 positive integers?",
            "45", "50", "55", "60", 2,
            "Using the formula n(n+1)/2 with n=10 gives 10 x 11 / 2 = 55."));
        list.Add(Q(Math, 3, 6, catMath, QuizDifficulty.Hard,
            "What kind of number, such as pi or the square root of 2, cannot be written as a simple fraction?",
            "Rational number", "Irrational number", "Complex number", "Natural number", 1,
            "Irrational numbers have non-terminating, non-repeating decimal expansions and cannot be written as a ratio of two integers."));

        // ---------------- History ----------------
        const string catHist = QuizCategories.History;

        list.Add(Q(Hist, 1, 1, catHist, QuizDifficulty.Easy,
            "In what year did World War II end?",
            "1943", "1945", "1947", "1950", 1,
            "World War II ended in 1945, with Germany surrendering in May and Japan in September."));
        list.Add(Q(Hist, 1, 2, catHist, QuizDifficulty.Easy,
            "Who was the first President of the United States?",
            "Thomas Jefferson", "John Adams", "George Washington", "Abraham Lincoln", 2,
            "George Washington served as the first U.S. President from 1789 to 1797."));
        list.Add(Q(Hist, 1, 3, catHist, QuizDifficulty.Easy,
            "Which ancient civilization built the pyramids of Giza?",
            "Romans", "Greeks", "Egyptians", "Mesopotamians", 2,
            "The ancient Egyptians built the Giza pyramids as tombs for their pharaohs around 2600-2500 BCE."));
        list.Add(Q(Hist, 1, 4, catHist, QuizDifficulty.Easy,
            "Which wall divided a European city for almost 30 years during the Cold War?",
            "The Great Wall", "The Berlin Wall", "Hadrian's Wall", "The Atlantic Wall", 1,
            "The Berlin Wall divided East and West Berlin from 1961 until it fell in 1989."));
        list.Add(Q(Hist, 1, 5, catHist, QuizDifficulty.Easy,
            "Who completed the famous 1492 transatlantic voyage credited with opening European contact with the Americas?",
            "Vasco da Gama", "Christopher Columbus", "Ferdinand Magellan", "Marco Polo", 1,
            "Christopher Columbus's 1492 voyage, sponsored by Spain, is traditionally credited with opening sustained European contact with the Americas."));
        list.Add(Q(Hist, 1, 6, catHist, QuizDifficulty.Easy,
            "Which ancient Roman structure was used for gladiator contests?",
            "The Parthenon", "The Colosseum", "The Pantheon", "The Acropolis", 1,
            "The Colosseum in Rome hosted gladiatorial combat and public spectacles beginning around 80 CE."));

        list.Add(Q(Hist, 2, 1, catHist, QuizDifficulty.Medium,
            "Julius Caesar was a general and statesman of which ancient civilization?",
            "Ottoman", "Roman", "Byzantine", "Persian", 1,
            "Julius Caesar was a Roman general and statesman who became dictator of the Roman Republic, paving the way for the Roman Empire that followed under Augustus."));
        list.Add(Q(Hist, 2, 2, catHist, QuizDifficulty.Medium,
            "The Magna Carta was signed in what year?",
            "1066", "1215", "1348", "1492", 1,
            "King John of England signed the Magna Carta in 1215, limiting royal power and establishing lasting legal principles."));
        list.Add(Q(Hist, 2, 3, catHist, QuizDifficulty.Medium,
            "Which country was the first to industrialize during the Industrial Revolution?",
            "France", "Germany", "United Kingdom", "United States", 2,
            "The Industrial Revolution began in Great Britain in the late 18th century, driven by innovations like the steam engine."));
        list.Add(Q(Hist, 2, 4, catHist, QuizDifficulty.Medium,
            "Who led the Soviet Union throughout most of World War II?",
            "Vladimir Lenin", "Joseph Stalin", "Nikita Khrushchev", "Leon Trotsky", 1,
            "Joseph Stalin led the Soviet Union throughout World War II, from its start in 1939 through the Allied victory in 1945."));
        list.Add(Q(Hist, 2, 5, catHist, QuizDifficulty.Medium,
            "The French Revolution began in what year?",
            "1776", "1789", "1804", "1815", 1,
            "The French Revolution began in 1789 with the storming of the Bastille and the collapse of the monarchy's absolute power."));
        list.Add(Q(Hist, 2, 6, catHist, QuizDifficulty.Medium,
            "Which explorer's expedition was the first to circumnavigate the globe?",
            "Christopher Columbus", "Ferdinand Magellan", "James Cook", "Vasco da Gama", 1,
            "Ferdinand Magellan's expedition, completed after his death by his surviving crew, was the first to circumnavigate the Earth, from 1519-1522."));

        list.Add(Q(Hist, 3, 1, catHist, QuizDifficulty.Hard,
            "The Peace of Westphalia (1648) is most significant for establishing which concept in international relations?",
            "Democracy", "State sovereignty", "Free trade", "Human rights", 1,
            "The Peace of Westphalia established the principle of state sovereignty, ending the Thirty Years' War and shaping the modern nation-state system."));
        list.Add(Q(Hist, 3, 2, catHist, QuizDifficulty.Hard,
            "Which dynasty first unified China under Qin Shi Huang?",
            "Han Dynasty", "Qin Dynasty", "Tang Dynasty", "Ming Dynasty", 1,
            "Qin Shi Huang unified the warring states of China under the Qin Dynasty in 221 BCE, becoming its first emperor."));
        list.Add(Q(Hist, 3, 3, catHist, QuizDifficulty.Hard,
            "Which event directly triggered the start of World War I?",
            "The bombing of Pearl Harbor", "The assassination of Archduke Franz Ferdinand", "The invasion of Poland", "The sinking of the Lusitania", 1,
            "The assassination of Archduke Franz Ferdinand of Austria-Hungary in Sarajevo in June 1914 set off the alliance system that led to World War I."));
        list.Add(Q(Hist, 3, 4, catHist, QuizDifficulty.Hard,
            "The Congress of Vienna (1814-1815) was convened primarily to do what?",
            "End slavery", "Redraw Europe's political map after Napoleon", "Establish the League of Nations", "Divide Africa among colonial powers", 1,
            "The Congress of Vienna reorganized Europe's borders and balance of power following the Napoleonic Wars."));
        list.Add(Q(Hist, 3, 5, catHist, QuizDifficulty.Hard,
            "Which ancient Mesopotamian king is known for one of the earliest surviving written law codes?",
            "Sargon of Akkad", "Hammurabi", "Nebuchadnezzar", "Cyrus the Great", 1,
            "Hammurabi, king of Babylon, issued the Code of Hammurabi around 1754 BCE, one of the earliest and most complete written legal codes."));
        list.Add(Q(Hist, 3, 6, catHist, QuizDifficulty.Hard,
            "The Meiji Restoration of 1868 marked a turning point in the history of which country?",
            "China", "Korea", "Japan", "Vietnam", 2,
            "The Meiji Restoration restored imperial rule in Japan and launched an era of rapid modernization and industrialization."));

        // ---------------- Geography ----------------
        const string catGeo = QuizCategories.Geography;

        list.Add(Q(Geo, 1, 1, catGeo, QuizDifficulty.Easy,
            "What is the largest continent by area?",
            "Africa", "Asia", "Europe", "North America", 1,
            "Asia is the largest continent, covering about 30% of Earth's total land area."));
        list.Add(Q(Geo, 1, 2, catGeo, QuizDifficulty.Easy,
            "Which river is traditionally considered the longest in the world?",
            "Amazon River", "Nile River", "Yangtze River", "Mississippi River", 1,
            "The Nile River, flowing through northeastern Africa, is traditionally considered the longest river at about 6,650 km."));
        list.Add(Q(Geo, 1, 3, catGeo, QuizDifficulty.Easy,
            "What is the capital city of France?",
            "Lyon", "Marseille", "Paris", "Nice", 2,
            "Paris has been the capital of France since the late 10th century."));
        list.Add(Q(Geo, 1, 4, catGeo, QuizDifficulty.Easy,
            "Which ocean is the largest in the world?",
            "Atlantic Ocean", "Indian Ocean", "Arctic Ocean", "Pacific Ocean", 3,
            "The Pacific Ocean is the largest and deepest ocean, covering more area than all of Earth's landmass combined."));
        list.Add(Q(Geo, 1, 5, catGeo, QuizDifficulty.Easy,
            "What is the smallest country in the world by area?",
            "Monaco", "Vatican City", "San Marino", "Liechtenstein", 1,
            "Vatican City, an independent city-state in Rome, covers about 0.44 square kilometers, making it the world's smallest country."));
        list.Add(Q(Geo, 1, 6, catGeo, QuizDifficulty.Easy,
            "Mount Everest is located in which mountain range?",
            "Andes", "Alps", "Himalayas", "Rockies", 2,
            "Mount Everest, Earth's highest peak, sits in the Himalayas on the border of Nepal and Tibet."));

        list.Add(Q(Geo, 2, 1, catGeo, QuizDifficulty.Medium,
            "Which desert is the largest hot desert in the world?",
            "Gobi Desert", "Kalahari Desert", "Sahara Desert", "Sonoran Desert", 2,
            "The Sahara Desert in North Africa is the largest hot desert, covering roughly 9.2 million square kilometers."));
        list.Add(Q(Geo, 2, 2, catGeo, QuizDifficulty.Medium,
            "Which country contains more natural lakes than any other?",
            "United States", "Russia", "Canada", "Finland", 2,
            "Canada contains more lakes than the rest of the world's countries combined, a legacy of glacial activity during the last ice age."));
        list.Add(Q(Geo, 2, 3, catGeo, QuizDifficulty.Medium,
            "What is the name of the imaginary line at 0 degrees longitude?",
            "Equator", "Tropic of Cancer", "Prime Meridian", "International Date Line", 2,
            "The Prime Meridian, passing through Greenwich, England, marks 0 degrees longitude and is the reference point for world time zones."));
        list.Add(Q(Geo, 2, 4, catGeo, QuizDifficulty.Medium,
            "Which African country was formerly known as Abyssinia?",
            "Kenya", "Ethiopia", "Sudan", "Somalia", 1,
            "Ethiopia was historically known as Abyssinia and is one of the oldest continuously independent countries in Africa."));
        list.Add(Q(Geo, 2, 5, catGeo, QuizDifficulty.Medium,
            "Which strait separates Europe and Africa at its narrowest point?",
            "Strait of Hormuz", "Strait of Gibraltar", "Bosphorus Strait", "Bering Strait", 1,
            "The Strait of Gibraltar separates Spain (Europe) from Morocco (Africa), narrowing to about 14 km wide."));
        list.Add(Q(Geo, 2, 6, catGeo, QuizDifficulty.Medium,
            "Which continent is technically the driest on Earth?",
            "Africa", "Australia", "Antarctica", "Asia", 2,
            "Antarctica is technically a polar desert, receiving very little precipitation despite its ice cover, making it the driest continent."));

        list.Add(Q(Geo, 3, 1, catGeo, QuizDifficulty.Hard,
            "Which country has the most time zones, including its overseas territories?",
            "Russia", "United States", "France", "China", 2,
            "France has 12 time zones because of its overseas territories scattered across the globe, more than any other country."));
        list.Add(Q(Geo, 3, 2, catGeo, QuizDifficulty.Hard,
            "What is the name of the horseshoe-shaped zone around the Pacific Ocean known for frequent earthquakes and volcanic activity?",
            "The Great Rift Valley", "The Ring of Fire", "The Mid-Atlantic Ridge", "The San Andreas Fault", 1,
            "The Ring of Fire traces tectonic plate boundaries around the Pacific Ocean, producing intense seismic and volcanic activity."));
        list.Add(Q(Geo, 3, 3, catGeo, QuizDifficulty.Hard,
            "Which landlocked body of water is actually the largest enclosed inland body of water on Earth?",
            "Dead Sea", "Caspian Sea", "Aral Sea", "Black Sea", 1,
            "The Caspian Sea, bordered by five countries, is the largest enclosed inland body of water on Earth by area."));
        list.Add(Q(Geo, 3, 4, catGeo, QuizDifficulty.Hard,
            "What primarily causes the Coriolis effect that influences wind and ocean current directions?",
            "Earth's magnetic field", "Earth's rotation", "The moon's gravity", "Solar radiation", 1,
            "The Coriolis effect arises from Earth's rotation, deflecting moving air and water to the right in the Northern Hemisphere and left in the Southern Hemisphere."));
        list.Add(Q(Geo, 3, 5, catGeo, QuizDifficulty.Hard,
            "Which country straddles both Europe and Asia across the Bosphorus?",
            "Russia", "Kazakhstan", "Turkey", "Georgia", 2,
            "Turkey spans two continents, with Istanbul straddling the Bosphorus strait that divides Europe and Asia."));
        list.Add(Q(Geo, 3, 6, catGeo, QuizDifficulty.Hard,
            "What is the term for a landmass entirely surrounded by water but smaller in scale than a continent?",
            "Peninsula", "Archipelago", "Island", "Isthmus", 2,
            "An island is a piece of land completely surrounded by water and smaller in scale than a continent."));

        // ---------------- General Knowledge ----------------
        const string catGk = QuizCategories.GeneralKnowledge;

        list.Add(Q(Gk, 1, 1, catGk, QuizDifficulty.Easy,
            "How many continents are there on Earth?",
            "5", "6", "7", "8", 2,
            "Earth has seven continents: Africa, Antarctica, Asia, Australia, Europe, North America, and South America."));
        list.Add(Q(Gk, 1, 2, catGk, QuizDifficulty.Easy,
            "What color do you get by mixing blue and yellow paint?",
            "Purple", "Orange", "Green", "Brown", 2,
            "Mixing blue and yellow pigments produces green."));
        list.Add(Q(Gk, 1, 3, catGk, QuizDifficulty.Easy,
            "How many days are there in a leap year?",
            "364", "365", "366", "367", 2,
            "A leap year has 366 days, with an extra day (February 29) added to keep the calendar aligned with Earth's orbit."));
        list.Add(Q(Gk, 1, 4, catGk, QuizDifficulty.Easy,
            "What is the largest mammal in the world?",
            "African elephant", "Blue whale", "Giraffe", "Polar bear", 1,
            "The blue whale is the largest animal known to have ever existed, reaching up to 30 meters in length."));
        list.Add(Q(Gk, 1, 5, catGk, QuizDifficulty.Easy,
            "Which instrument has keys, pedals, and strings, and is played by pressing its keys?",
            "Violin", "Piano", "Flute", "Drum", 1,
            "The piano produces sound when pressed keys cause hammers to strike strings inside the instrument."));
        list.Add(Q(Gk, 1, 6, catGk, QuizDifficulty.Easy,
            "What sweet food do bees produce from flower nectar?",
            "Milk", "Honey", "Silk", "Butter", 1,
            "Bees produce honey from flower nectar, storing it as food inside their hive."));

        list.Add(Q(Gk, 2, 1, catGk, QuizDifficulty.Medium,
            "Who wrote the play \"Romeo and Juliet\"?",
            "Charles Dickens", "William Shakespeare", "Mark Twain", "Jane Austen", 1,
            "William Shakespeare wrote \"Romeo and Juliet\" around 1594-1596, one of his most famous tragedies."));
        list.Add(Q(Gk, 2, 2, catGk, QuizDifficulty.Medium,
            "What is the national currency of Japan?",
            "Won", "Yuan", "Yen", "Ringgit", 2,
            "The Japanese yen is the official currency of Japan."));
        list.Add(Q(Gk, 2, 3, catGk, QuizDifficulty.Medium,
            "Which planet is best known for its prominent, easily visible ring system?",
            "Mars", "Saturn", "Neptune", "Mercury", 1,
            "Saturn's rings, made mostly of ice and rock particles, are the most extensive and visible ring system in the solar system."));
        list.Add(Q(Gk, 2, 4, catGk, QuizDifficulty.Medium,
            "In Greek mythology, who is the king of the gods?",
            "Poseidon", "Hades", "Zeus", "Apollo", 2,
            "Zeus is the king of the Olympian gods in Greek mythology, ruling over the sky and thunder."));
        list.Add(Q(Gk, 2, 5, catGk, QuizDifficulty.Medium,
            "What is the primary ingredient in traditional guacamole?",
            "Tomato", "Avocado", "Onion", "Lime", 1,
            "Guacamole is primarily made from mashed avocado, often mixed with lime, onion, and seasonings."));
        list.Add(Q(Gk, 2, 6, catGk, QuizDifficulty.Medium,
            "In which sport is the term \"love\" used to mean a score of zero?",
            "Basketball", "Tennis", "Cricket", "Golf", 1,
            "In tennis, \"love\" is the traditional term used for a score of zero."));

        list.Add(Q(Gk, 3, 1, catGk, QuizDifficulty.Hard,
            "What is the scientific study of fungi called?",
            "Botany", "Mycology", "Entomology", "Herpetology", 1,
            "Mycology is the branch of biology dedicated to the study of fungi."));
        list.Add(Q(Gk, 3, 2, catGk, QuizDifficulty.Hard,
            "Which philosopher wrote \"The Republic\", exploring justice and the ideal state?",
            "Aristotle", "Socrates", "Plato", "Epicurus", 2,
            "Plato authored \"The Republic\", a foundational work of Western philosophy examining justice and the ideal society through dialogues."));
        list.Add(Q(Gk, 3, 3, catGk, QuizDifficulty.Hard,
            "What is the term for a word that reads the same forwards and backwards?",
            "Anagram", "Palindrome", "Acronym", "Homograph", 1,
            "A palindrome reads the same forward and backward, like \"level\" or \"racecar\"."));
        list.Add(Q(Gk, 3, 4, catGk, QuizDifficulty.Hard,
            "Which organization administers the awarding of the Nobel Prizes?",
            "United Nations", "The Nobel Foundation", "UNESCO", "The Red Cross", 1,
            "The Nobel Foundation, based in Sweden, manages the assets and administers the awarding of the Nobel Prizes established by Alfred Nobel's will."));
        list.Add(Q(Gk, 3, 5, catGk, QuizDifficulty.Hard,
            "What is a group of crows traditionally called?",
            "A flock", "A murder", "A parliament", "A pod", 1,
            "A group of crows is traditionally called \"a murder\", a term rooted in old folklore about the birds."));
        list.Add(Q(Gk, 3, 6, catGk, QuizDifficulty.Hard,
            "Which artist painted the ceiling of the Sistine Chapel?",
            "Leonardo da Vinci", "Raphael", "Michelangelo", "Donatello", 2,
            "Michelangelo painted the Sistine Chapel ceiling between 1508 and 1512, including the iconic \"Creation of Adam\"."));

        return list;
    }
}
