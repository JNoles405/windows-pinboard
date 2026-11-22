'use client';

import { useEffect, useMemo, useState } from 'react';
import { BANDS, WEATHER, SCENES, DAY } from '../lib/city';
import { bandForHour, seededWeather, fetchRealWeather, msUntilDusk } from '../lib/conditions';

const STORAGE_KEY = 'wick:seen';

function loadSeen() {
  try {
    return new Set(JSON.parse(localStorage.getItem(STORAGE_KEY) || '[]'));
  } catch {
    return new Set();
  }
}

function saveSeen(set) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify([...set]));
  } catch {}
}

// Debug overrides: ?hour=2&weather=rain
function readOverrides() {
  if (typeof window === 'undefined') return {};
  const p = new URLSearchParams(window.location.search);
  const hour = p.has('hour') ? Number(p.get('hour')) : undefined;
  const weather = p.get('weather') || undefined;
  return {
    hour: Number.isInteger(hour) && hour >= 0 && hour < 24 ? hour : undefined,
    weather: WEATHER[weather] ? weather : undefined,
  };
}

function Scene({ text, hint }) {
  const [open, setOpen] = useState(false);
  const parts = text.split(/\[\[(.+?)\]\]/);
  return (
    <>
      <p className="prose">
        {parts.map((part, i) =>
          i % 2 === 1 ? (
            <button
              key={i}
              className={`clue ${open ? 'clue-open' : ''}`}
              onClick={() => setOpen((o) => !o)}
              aria-expanded={open}
            >
              {part}
            </button>
          ) : (
            <span key={i}>{part}</span>
          )
        )}
      </p>
      <p className={`hint ${open ? 'hint-open' : ''}`} aria-hidden={!open}>
        {hint}
      </p>
    </>
  );
}

function formatCountdown(ms) {
  const h = Math.floor(ms / 3600000);
  const m = Math.floor((ms % 3600000) / 60000);
  return `${h}h ${String(m).padStart(2, '0')}m`;
}

export default function Page() {
  const [now, setNow] = useState(null);
  const [weather, setWeather] = useState(null);
  const [weatherSource, setWeatherSource] = useState('');
  const [seen, setSeen] = useState(new Set());
  const overrides = useMemo(readOverrides, []);

  useEffect(() => {
    setNow(new Date());
    setSeen(loadSeen());
    const t = setInterval(() => setNow(new Date()), 30000);
    return () => clearInterval(t);
  }, []);

  useEffect(() => {
    if (overrides.weather) {
      setWeather(overrides.weather);
      setWeatherSource('override');
      return;
    }
    let cancelled = false;
    fetchRealWeather()
      .then((w) => {
        if (!cancelled) {
          setWeather(w);
          setWeatherSource('sky');
        }
      })
      .catch(() => {
        if (!cancelled) {
          setWeather(seededWeather());
          setWeatherSource('guess');
        }
      });
    return () => {
      cancelled = true;
    };
  }, [overrides.weather]);

  const hour = overrides.hour ?? now?.getHours();
  const band = hour === undefined ? undefined : bandForHour(hour);
  const scene = band && weather ? SCENES[band][weather] : null;

  useEffect(() => {
    if (!scene) return;
    const key = `${band}:${weather}`;
    setSeen((prev) => {
      if (prev.has(key)) return prev;
      const next = new Set(prev).add(key);
      saveSeen(next);
      return next;
    });
  }, [band, weather, scene]);

  // Still waking up
  if (!now || (band && !weather)) {
    return (
      <main className="stage band-loading">
        <p className="loading">The street is arriving…</p>
      </main>
    );
  }

  // Daytime
  if (!band) {
    return (
      <main className="stage band-day">
        <article className="card">
          <h1 className="title">{DAY.title}</h1>
          <p className="prose">{DAY.text}</p>
          <p className="meta">Returns in {formatCountdown(msUntilDusk(now))}</p>
        </article>
      </main>
    );
  }

  return (
    <main className={`stage band-${band} weather-${weather}`}>
      {weather === 'rain' && <div className="rain" aria-hidden />}
      {weather === 'fog' && <div className="fog" aria-hidden />}
      <article className="card">
        <p className="kicker">
          Tallow Street · {BANDS[band].name} · {WEATHER[weather]}
        </p>
        <h1 className="title">{scene.title}</h1>
        <Scene key={`${band}:${weather}`} text={scene.text} hint={scene.hint} />
        <footer className="meta">
          <span>
            {seen.size} of 12 faces of Tallow Street seen
            {seen.size === 12 ? '. Pell’s map is true now.' : ''}
          </span>
          {weatherSource === 'guess' && (
            <span className="sub">Wick can&rsquo;t see your sky, so it&rsquo;s guessing.</span>
          )}
        </footer>
      </article>
    </main>
  );
}
