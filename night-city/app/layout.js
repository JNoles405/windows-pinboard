import './globals.css';

export const metadata = {
  title: 'Wick',
  description: 'A city that only exists at night.',
};

export const viewport = {
  width: 'device-width',
  initialScale: 1,
  themeColor: '#07080d',
};

export default function RootLayout({ children }) {
  return (
    <html lang="en">
      <head>
        <link rel="preconnect" href="https://fonts.googleapis.com" />
        <link rel="preconnect" href="https://fonts.gstatic.com" crossOrigin="" />
        <link
          href="https://fonts.googleapis.com/css2?family=Cormorant+Garamond:ital,wght@0,400;0,600;1,400&family=IM+Fell+English+SC&display=swap"
          rel="stylesheet"
        />
      </head>
      <body>{children}</body>
    </html>
  );
}
