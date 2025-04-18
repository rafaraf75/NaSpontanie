# NaSpontanie

Aplikacja mobilna stworzona w .NET MAUI umożliwiająca użytkownikom znajdowanie wydarzeń w okolicy, dołączanie do nich oraz interakcję z innymi uczestnikami.

## Opis

NaSpontanie to system składający się z:

- **Aplikacji mobilnej** (MAUI) do przeglądania i tworzenia wydarzeń.
- **REST API** opartego na ASP.NET Core, który obsługuje użytkowników, wydarzenia, komentarze, znajomych itd.

Użytkownicy mogą:
- Zakładać konto i logować się
- Dodawać wydarzenia z lokalizacją
- Przeglądać wydarzenia na liście i mapie
- Dołączać do wydarzeń i komentować
- Dodawać znajomych i zarządzać profilem
- Zarządzać komentarzami (dodawać, usuwać, zgłaszać do moderacji)

## Wymagania

- Visual Studio 2022 z obsługą .NET MAUI
- Emulator Androida / fizyczne urządzenie
- Konto z kluczem Google Maps API

## Uruchamianie projektu

1. Sklonuj repozytorium:
   git clone https://github.com/rafaraf75/NaSpontanie.git
2. Otwórz rozwiązanie NaSpontanie.sln
3. Uruchom projekt NaSpontanie.API (REST API)
4. Uruchom NaSpontanie.MAUI(aplikacja mobilna na emulatorze lub urzadzeniu)
5. Utwórz plik secrets.xml w folderze:
   NaSpontanie.MAUI/Platforms/Android/Resources/values/secrets.xml
6. Wklej do niego swój klucz Google Maps API:
   <resources>
     <string name="google_maps_key">TU_WKLEJ_SWÓJ_KLUCZ</string>
   </resources>
**Uwaga:** Klucz Google Maps API jest wymagany do prawidłowego działania aplikacji.

   
   
