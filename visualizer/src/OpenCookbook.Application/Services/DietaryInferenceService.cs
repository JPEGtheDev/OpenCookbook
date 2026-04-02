namespace OpenCookbook.Application.Services;

/// <summary>
/// Infers dietary attributes (Dairy Free, Vegetarian, Vegan) from a list of
/// ingredient names using keyword matching.
///
/// Rules:
/// - DairyFree  : true when no dairy keyword is found in any ingredient name.
/// - Vegetarian : true when no meat keyword is found in any ingredient name.
/// - Vegan      : true when no meat, dairy, or other animal-derived keyword is found.
///
/// If ANY ingredient name cannot be classified (unknown), the corresponding
/// attribute is returned as null — the recipe is excluded from that dietary filter
/// so that recipes are never incorrectly promoted.
/// </summary>
public sealed class DietaryInferenceService
{
    // ── keyword lists ───────────────────────────────────────────────────────

    private static readonly HashSet<string> MeatKeywords =
    [
        "beef", "ground beef", "chuck", "brisket", "steak", "short rib",
        "chicken", "turkey", "duck", "goose", "quail",
        "pork", "bacon", "ham", "sausage", "salami", "pepperoni", "prosciutto",
        "lamb", "mutton", "veal",
        "fish", "salmon", "tuna", "cod", "halibut", "tilapia", "trout", "mahi",
        "shrimp", "prawn", "lobster", "crab", "scallop", "clam", "oyster",
        "meat", "poultry", "seafood", "venison", "bison", "rabbit",
        "anchovies", "anchovy", "sardine", "herring", "mackerel"
    ];

    private static readonly HashSet<string> DairyKeywords =
    [
        "milk", "whole milk", "skim milk",
        "cream", "heavy cream", "sour cream", "whipping cream",
        "butter",
        "cheese", "cheddar", "mozzarella", "parmesan", "feta", "brie", "gouda",
        "ricotta", "cottage cheese", "cream cheese",
        "yogurt", "kefir",
        "whey", "casein", "lactose",
        "ghee"
    ];

    /// <summary>
    /// Plant-based dairy alternatives that contain dairy keywords (e.g. "milk")
    /// but are not animal-derived. Checked before the dairy keyword list.
    /// </summary>
    private static readonly HashSet<string> PlantDairyAlternatives =
    [
        "almond milk", "oat milk", "soy milk", "rice milk", "coconut milk",
        "coconut cream", "cashew milk", "hemp milk", "pea milk"
    ];

    /// <summary>
    /// Animal-derived ingredients that are neither meat nor dairy
    /// (e.g. eggs, honey, gelatin, lard). These affect Vegan status only.
    /// </summary>
    private static readonly HashSet<string> OtherAnimalDerivedKeywords =
    [
        "egg", "eggs", "egg white", "egg yolk", "egg whites", "egg yolks",
        "honey", "beeswax",
        "gelatin", "gelatine",
        "lard", "tallow", "suet",
        "worcestershire sauce"  // typically contains anchovies
    ];

    // ── public API ──────────────────────────────────────────────────────────

    /// <summary>
    /// Infers a <see cref="DietaryProfile"/> for the given ingredient names.
    /// </summary>
    public DietaryProfile Infer(IEnumerable<string> ingredientNames)
    {
        var names = ingredientNames.ToList();

        if (names.Count == 0)
            return new DietaryProfile(); // all null → unknown

        bool hasMeat = false;
        bool hasDairy = false;
        bool hasOtherAnimal = false;
        bool hasUnclassified = false;

        foreach (var name in names)
        {
            var category = Classify(name);
            switch (category)
            {
                case IngredientCategory.Meat:
                    hasMeat = true;
                    break;
                case IngredientCategory.Dairy:
                    hasDairy = true;
                    break;
                case IngredientCategory.OtherAnimalDerived:
                    hasOtherAnimal = true;
                    break;
                case IngredientCategory.Unknown:
                    hasUnclassified = true;
                    break;
                // PlantBased: no flags set
            }
        }

        return new DietaryProfile
        {
            // Dairy Free: null (unknown) when there are unclassified ingredients
            // and no dairy was detected (we can't be sure it's dairy free).
            IsDairyFree = hasDairy ? false
                        : hasUnclassified ? null
                        : true,

            // Vegetarian: null when unclassified and no meat was detected.
            IsVegetarian = hasMeat ? false
                         : hasUnclassified ? null
                         : true,

            // Vegan: null when unclassified and no animal-derived was detected.
            IsVegan = (hasMeat || hasDairy || hasOtherAnimal) ? false
                    : hasUnclassified ? null
                    : true
        };
    }

    // ── private helpers ─────────────────────────────────────────────────────

    internal IngredientCategory Classify(string ingredientName)
    {
        var lower = ingredientName.Trim().ToLowerInvariant();

        // Plant-based dairy alternatives must be checked first so that
        // "almond milk" and "coconut milk" are not classified as dairy.
        if (PlantDairyAlternatives.Any(alt => lower.Contains(alt, StringComparison.Ordinal)))
            return IngredientCategory.PlantBased;

        if (ContainsKeyword(lower, MeatKeywords))
            return IngredientCategory.Meat;

        if (ContainsKeyword(lower, DairyKeywords))
            return IngredientCategory.Dairy;

        if (ContainsKeyword(lower, OtherAnimalDerivedKeywords))
            return IngredientCategory.OtherAnimalDerived;

        // If the name is a recognisable plant-based item we return PlantBased;
        // otherwise Unknown (so the caller knows it couldn't be classified).
        if (IsLikelyPlantBased(lower))
            return IngredientCategory.PlantBased;

        return IngredientCategory.Unknown;
    }

    private static bool ContainsKeyword(string lower, HashSet<string> keywords)
    {
        // Check whether any keyword appears as a word-boundary match.
        foreach (var kw in keywords)
        {
            int idx = lower.IndexOf(kw, StringComparison.Ordinal);
            if (idx < 0)
                continue;

            bool startOk = idx == 0 || !char.IsLetter(lower[idx - 1]);
            bool endOk = idx + kw.Length == lower.Length || !char.IsLetter(lower[idx + kw.Length]);
            if (startOk && endOk)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true for ingredient names that are clearly plant-based
    /// (vegetables, fruits, grains, legumes, oils, spices, herbs, etc.).
    /// This list intentionally covers a broad range to minimize false "Unknown" results.
    /// Uses simple substring matching (no strict word boundary) to handle
    /// plurals and compound names (e.g. "potatoes", "tomato paste").
    /// </summary>
    private static bool IsLikelyPlantBased(string lower)
    {
        // Spices & herbs
        string[] spicesAndHerbs =
        [
            "salt", "pepper", "paprika", "cumin", "coriander", "turmeric", "cinnamon",
            "cardamom", "clove", "nutmeg", "allspice", "oregano", "thyme", "rosemary",
            "basil", "parsley", "dill", "mint", "bay leaf", "chili", "cayenne", "sumac",
            "za'atar", "fenugreek", "mustard seed", "anise", "fennel", "saffron",
            "curry", "garam masala", "seven spice", "baharat", "aleppo", "smoked paprika"
        ];

        // Vegetables
        string[] vegetables =
        [
            "onion", "garlic", "tomato", "potato", "carrot", "celery", "pepper",
            "capsicum", "zucchini", "eggplant", "aubergine", "spinach", "kale",
            "broccoli", "cauliflower", "cabbage", "lettuce", "cucumber", "corn",
            "pea", "bean", "lentil", "chickpea", "leek", "shallot", "scallion",
            "mushroom", "asparagus", "artichoke", "beet", "turnip", "parsnip",
            "sweet potato", "yam", "radish", "fennel bulb", "bok choy", "arugula"
        ];

        // Fruits
        string[] fruits =
        [
            "lemon", "lime", "orange", "apple", "banana", "grape", "pear",
            "cherry", "peach", "mango", "pineapple", "strawberry", "blueberry",
            "raspberry", "avocado", "olive", "pomegranate", "fig", "date",
            "raisin", "cranberry", "coconut"
        ];

        // Grains & starches
        string[] grains =
        [
            "flour", "bread", "wheat", "rice", "pasta", "oat", "barley", "quinoa",
            "cornstarch", "breadcrumb", "panko", "semolina", "bulgur", "couscous",
            "noodle", "tortilla", "pita", "cracker"
        ];

        // Oils, vinegars, condiments
        string[] condiments =
        [
            "oil", "olive oil", "vegetable oil", "sunflower oil", "canola oil",
            "vinegar", "soy sauce", "tahini", "tomato paste", "tomato sauce",
            "ketchup", "mustard", "hot sauce", "sriracha", "miso", "nutritional yeast",
            "sugar", "syrup", "maple", "molasses", "baking powder", "baking soda",
            "yeast", "vanilla"
        ];

        // Nuts & seeds
        string[] nutsAndSeeds =
        [
            "almond", "walnut", "cashew", "pecan", "pistachio", "hazelnut",
            "peanut", "pine nut", "sesame", "flaxseed", "chia", "sunflower seed",
            "pumpkin seed", "hemp seed"
        ];

        // Non-dairy milk alternatives & soy products
        string[] plantMilks =
        [
            "almond milk", "oat milk", "soy milk", "coconut milk", "rice milk",
            "coconut cream", "tofu", "tempeh", "seitan"
        ];

        return spicesAndHerbs.Any(kw => lower.Contains(kw, StringComparison.Ordinal))
            || vegetables.Any(kw => lower.Contains(kw, StringComparison.Ordinal))
            || fruits.Any(kw => lower.Contains(kw, StringComparison.Ordinal))
            || grains.Any(kw => lower.Contains(kw, StringComparison.Ordinal))
            || condiments.Any(kw => lower.Contains(kw, StringComparison.Ordinal))
            || nutsAndSeeds.Any(kw => lower.Contains(kw, StringComparison.Ordinal))
            || plantMilks.Any(kw => lower.Contains(kw, StringComparison.Ordinal));
    }
}

// ── supporting types ─────────────────────────────────────────────────────────

public enum IngredientCategory
{
    Unknown,
    Meat,
    Dairy,
    OtherAnimalDerived,
    PlantBased
}

public sealed class DietaryProfile
{
    /// <summary>True if the recipe contains no dairy. Null if unknown.</summary>
    public bool? IsDairyFree { get; init; }

    /// <summary>True if the recipe contains no meat. Null if unknown.</summary>
    public bool? IsVegetarian { get; init; }

    /// <summary>True if the recipe contains no animal-derived ingredients. Null if unknown.</summary>
    public bool? IsVegan { get; init; }
}
