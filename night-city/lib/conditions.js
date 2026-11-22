// Local hour -> band. Returns null during the day (6:00–18:59).
export function bandForHour(h) {
  if (h >= 19 && h < 22) return 'lamplight';
  if (h >= 22 || h < 1) return 'longhour';
  if (h >= 1 && h < 4) return 'lowtide';
  if (h >= 4 && h < 6) return 'thinning';
  return null;
}

// WMO weather codes (Open-Meteo) -> clear | rain | fog
export function weatherFromCode(code) {
  if ([51, 53, 55, 56, 57, 61, 63, 65, 66, 67, 80, 81, 82, 95, 96, 99].includes(code)) return 'rain';
  if ([3, 45, 48, 71, 73, 75, 77, 85, 86].includes(code)) return 'fog';
  return 'clear';
}

// Fallback when location is denied: stable per night, varies night to night.
export function seededWeather(date = new Date()) {
  const night = new Date(date.getTime() - 6 * 3600 * 1000); // night belongs to the evening it started
  const seed = night.getFullYear() * 1000 + night.getMonth() * 40 + night.getDate();
  const hash = Math.imul(seed, 2654435761) >>> 0;
  return ['clear', 'rain', 'fog'][hash % 3];
}

function getPosition(timeoutMs = 6000) {
  return new Promise((resolve, reject) => {
    if (!('geolocation' in navigator)) return reject(new Error('no geolocation'));
    navigator.geolocation.getCurrentPosition(resolve, reject, {
      timeout: timeoutMs,
      maximumAge: 30 * 60 * 1000,
    });
  });
}

export async function fetchRealWeather() {
  const pos = await getPosition();
  const { latitude, longitude } = pos.coords;
  const url = `https://api.open-meteo.com/v1/forecast?latitude=${latitude.toFixed(2)}&longitude=${longitude.toFixed(2)}&current=weather_code`;
  const res = await fetch(url);
  if (!res.ok) throw new Error('weather fetch failed');
  const data = await res.json();
  return weatherFromCode(data?.current?.weather_code);
}

// Next 19:00 local time.
export function msUntilDusk(now = new Date()) {
  const dusk = new Date(now);
  dusk.setHours(19, 0, 0, 0);
  if (dusk <= now) dusk.setDate(dusk.getDate() + 1);
  return dusk - now;
}
