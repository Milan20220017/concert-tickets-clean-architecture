export interface Concert {
    id: number
    name: string
    date: string
    categoryId: number
    categoryName: string
    locationId: number
    locationName: string
    EarlyBirdDiscountUntil: string | null
}