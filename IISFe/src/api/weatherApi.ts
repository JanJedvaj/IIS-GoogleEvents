import { Code, ConnectError } from '@connectrpc/connect';
import { weatherClient } from './weatherGrpcClient';
import type { WeatherReadingsDto } from '../types/models';

const EMPTY: WeatherReadingsDto = { results: [], lastUpdated: '' };

export async function getWeatherByCity(cityName: string): Promise<WeatherReadingsDto> {
  try {
    const response = await weatherClient.getWeatherByCity({ cityName });

    return {
      lastUpdated: response.lastUpdated,
      results: response.results.map((reading) => ({
        cityName: reading.cityName,
        temperature: reading.temperature,
        humidity: reading.humidity,
        pressure: reading.pressure,
        windDirection: reading.windDirection,
        windSpeed: reading.windSpeed,
        condition: reading.condition,
      })),
    };
  } catch (error) {
    if (error instanceof ConnectError && error.code === Code.NotFound) return EMPTY;
    throw error;
  }
}
