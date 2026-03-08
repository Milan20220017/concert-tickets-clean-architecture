import { apiGet } from "#/lib/api";
import type {Region} from "#/types/region";

export async function getRegionsByLocationId(locationId: number): Promise<Region[]> {
    return apiGet<Region[]>(`/locations/${locationId}/regions`);
}