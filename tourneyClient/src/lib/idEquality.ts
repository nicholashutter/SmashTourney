// Compares two identifier strings using normalized GUID-compatible formatting.
export const areSameId = (left: string | null | undefined, right: string | null | undefined): boolean =>
{
    if (!left || !right)
    {
        return false;
    }

    return left.trim().toLowerCase() === right.trim().toLowerCase();
};
