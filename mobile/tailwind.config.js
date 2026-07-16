/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: "class",
  content: [
    "./app/**/*.{js,jsx,ts,tsx}",
    "./src/**/*.{js,jsx,ts,tsx}",
  ],
  presets: [require("nativewind/preset")],
  theme: {
    extend: {
      colors: {
        background: {
          DEFAULT: "#FFFFFF",
          dark: "#0B0E14",
        },
        surface: {
          DEFAULT: "#F5F6FA",
          dark: "#151923",
        },
        border: {
          DEFAULT: "#E4E7EC",
          dark: "#262B38",
        },
        // Named "ink" (not "text") so generated utilities read as
        // `text-ink-primary` / `dark:text-ink-primary-dark` instead of the
        // confusing `text-text-primary`.
        ink: {
          primary: "#12141A",
          "primary-dark": "#F3F4F6",
          secondary: "#5B6272",
          "secondary-dark": "#9AA1B1",
        },
        brand: {
          DEFAULT: "#5B5BF7",
          light: "#8E8CFB",
          dark: "#4139D9",
        },
        accent: {
          DEFAULT: "#00C2A8",
        },
        success: "#1E9E5A",
        warning: "#D98C0C",
        danger: "#E5484D",
      },
      spacing: {
        1: "4px",
        2: "8px",
        3: "12px",
        4: "16px",
        5: "20px",
        6: "24px",
        7: "28px",
        8: "32px",
        9: "36px",
        10: "40px",
        12: "48px",
        16: "64px",
      },
      borderRadius: {
        sm: "6px",
        md: "10px",
        lg: "16px",
        xl: "24px",
        full: "9999px",
      },
      fontSize: {
        display: ["34px", { lineHeight: "40px", fontWeight: "700" }],
        heading: ["24px", { lineHeight: "30px", fontWeight: "700" }],
        subheading: ["18px", { lineHeight: "24px", fontWeight: "600" }],
        body: ["16px", { lineHeight: "22px", fontWeight: "400" }],
        caption: ["13px", { lineHeight: "18px", fontWeight: "400" }],
      },
      // Web-only reference for the elevation scale in `src/theme/elevation.ts`.
      // NativeWind's `shadow-*` utilities are unreliable on native (RN needs
      // shadowColor/Offset/Opacity/Radius + Android `elevation`), so native
      // code should use `useElevation()`/`elevationStyles` instead of these
      // classes. These exist only so the same visual scale is available to
      // any web-rendered NativeWind className that wants a CSS box-shadow.
      boxShadow: {
        sm: "0 1px 2px rgba(18, 20, 26, 0.08)",
        md: "0 4px 8px rgba(18, 20, 26, 0.12)",
        lg: "0 10px 24px rgba(18, 20, 26, 0.16)",
      },
    },
  },
  plugins: [],
};
