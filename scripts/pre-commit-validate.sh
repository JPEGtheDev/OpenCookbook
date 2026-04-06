#!/usr/bin/env bash
# Pre-commit hook to validate recipe nutrition IDs
# Install with: ln -s ../../scripts/pre-commit-validate.sh .git/hooks/pre-commit

# Find repo root by looking for .git directory
REPO_ROOT=$(git rev-parse --show-toplevel)

# Run the validation script
python3 "$REPO_ROOT/scripts/validate-recipe-nutrition.py"
EXIT_CODE=$?

if [ "$EXIT_CODE" -eq 0 ]; then
    exit 0
elif [ "$EXIT_CODE" -eq 1 ]; then
    echo ""
    echo "❌ Pre-commit hook: Recipe validation failed"
    echo "Run 'python3 scripts/validate-recipe-nutrition.py' for details"
    exit "$EXIT_CODE"
else
    echo ""
    echo "❌ Pre-commit hook: Recipe validator could not run correctly (exit code: $EXIT_CODE)"
    echo "Check Python/runtime dependencies and run 'python3 scripts/validate-recipe-nutrition.py' for details"
    exit "$EXIT_CODE"
fi

