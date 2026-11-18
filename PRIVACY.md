# ScornholeXR Privacy Policy

Effective date: November 18, 2026

ScornholeXR is developed and operated by yellboy, an independent individual developer. This policy explains how information is processed when you use ScornholeXR on Meta Quest, including competition and test releases.

For privacy questions or requests concerning your information, contact [bokijelaca@gmail.com](mailto:bokijelaca@gmail.com).

## 1. Overview

ScornholeXR is a two-player online VR game. It uses Photon Fusion to connect players and synchronize gameplay, and OpenAI through Meta XR AI Building Blocks to generate ranking-based questions.

- You do not create a separate ScornholeXR account.
- The game does not include advertising, voice chat, free-text chat, or a developer-operated analytics service.
- I do not operate a separate server or database that stores player profiles or match histories.
- Online play and question generation require data to be processed by external services. This does not mean that the game processes no personal information.

## 2. Information processed and why

### XR tracking and interaction

The headset's XR runtime and the game process head position and orientation, controller input, and supported hand-tracking information to render the experience and let you interact with menus, grab bags, and throw them.

The game uses this information for real-time interaction, not to identify you through biometric characteristics or build a behavioral profile. The custom gameplay code does not store raw head or hand-tracking histories or include them in AI requests. Resulting bag movement and throw outcomes are shared as multiplayer gameplay data.

The current game does not implement microphone recording, camera-image capture, or transmission of images of your surroundings for question generation. Meta's device-level tracking and processing are governed separately by its privacy notices.

### Multiplayer information

Photon processes the information needed to establish and maintain a multiplayer session. This includes network connection information such as your IP address, service-generated identifiers, session and player identifiers, connection region, and technical connection data.

The game sends gameplay information through Photon to the other player, including:

- Bag positions, rotations, and physics state.
- Selected answer numbers and throw outcomes.
- Player assignments, turn order, and throw counts.
- Round settings, scores, and match results.
- Generated questions, answer options, and rankings.

This processing allows both players to participate in the same match. The other player can see your in-game actions and scores. The game does not ask you for your real name, email address, or a public player profile to join a match.

### AI question generation

The client responsible for running the match sends a predefined question-generation prompt to OpenAI. The request also includes previously generated questions, answer options, rankings, and source text from the current running game to help avoid repeated questions.

The game's question-generation code does not add your name, contact details, Meta account information, scores, selected answers, or raw XR tracking data to this prompt. There is no player text-entry feature for AI prompts.

OpenAI receives the request content and associated network and service metadata, which can include the requesting device's IP address. It generates the question and answer ranking, which the game then shares with both players.

OpenAI may retain API requests, responses, and technical records under its service terms, account settings, and data-retention policies. This policy does not promise zero retention or that all request data disappears when a match ends.

### Diagnostics and local logs

The game generates diagnostic messages about connections, turns, collisions, and errors. These may contain session identifiers, gameplay information, and generated question text when an error occurs.

The in-game debug panel keeps up to 500 displayed log entries in memory and provides a Clear button. Unity or the device may also maintain separate logs; clearing the panel does not delete those logs.

The game's custom code does not automatically upload this debug log to a developer-operated server. Unity engine diagnostics are enabled in the current project configuration, and engine or platform services may process technical application, device, and error information under their own privacy notices. Separate Unity Analytics and Unity Ads services are not enabled.

### Information you send to me

If you contact me for support or a privacy request, I receive your email address and whatever information you choose to provide, such as a description of an issue or diagnostic logs. I use this information to respond, investigate the issue, and handle your request.

Please do not send passwords, API keys, or unnecessary personal information. Review screenshots and logs before sharing them.

## 3. Service providers and disclosures

The following services support the experience:

- Photon / Exit Games: multiplayer connectivity and gameplay synchronization. See the [Photon Privacy and Cookie Policy](https://www.photonengine.com/terms/privacyandcookiepolicy).
- OpenAI: AI question generation. See the [OpenAI Privacy Policy](https://openai.com/policies/privacy-policy/) and [API data controls documentation](https://developers.openai.com/api/docs/guides/your-data).
- Meta: headset operation, XR services, accounts, and app distribution. See the [Meta Privacy Policy](https://www.facebook.com/privacy/policy/) and [Supplemental Meta Platforms Technologies Privacy Policy](https://www.meta.com/legal/privacy-policy/).
- Unity: the game engine and applicable engine-level services and diagnostics. See the [Unity Game Player and App User Privacy Policy](https://unity.com/legal/game-player-and-app-user-privacy-policy).

These notices explain the providers' own practices. Depending on the service and purpose, a provider may process data on behalf of the developer or for its own platform, security, and operational purposes.

Your Meta account and any store transaction are handled by Meta; ScornholeXR does not request your Meta password or payment-card details.

I do not sell personal information or share it for cross-context behavioral advertising. Information may also be disclosed where required by law, to respond to valid legal requests, or where necessary to protect users, investigate misuse, or protect legal rights.

## 4. Retention

- Gameplay state, generated-question history, and the in-game debug buffer are held in application memory rather than saved as a developer-maintained player history. Closing the application ends that in-memory state.
- Local engine or device logs can persist beyond a session and follow the relevant system's storage and cleanup behavior.
- Photon, OpenAI, Meta, and Unity may retain operational records or other data according to the applicable service, configuration, legal requirements, and provider policies. Ending a match or uninstalling the app does not automatically delete their records.
- Support correspondence is retained only as long as reasonably needed to resolve the request, handle related follow-up, and meet applicable legal obligations or resolve disputes. You may request deletion of information I hold.

## 5. Your choices and privacy rights

You may stop the game's ongoing gameplay processing by closing the application. You can manage available permissions and privacy controls through your Meta account and device settings. Restricting required XR or network functionality may prevent the game from working.

Depending on your location, you may have rights to access, correct, delete, or receive a copy of your personal information, restrict or object to processing, and withdraw consent where processing relies on consent. You may also have the right to complain to a data-protection authority.

To make a request concerning information I control, email [bokijelaca@gmail.com](mailto:bokijelaca@gmail.com). Describe the request and provide only the information reasonably necessary to identify the relevant interaction, such as the email address used for support or an approximate session date. Do not send passwords or API keys.

I may need to verify your request before acting on it and will respond within the time required by applicable law. Because the game does not maintain named player accounts or a developer-held match-history database, I may not be able to associate a past session with a particular person.

For information processed on my behalf by a provider, I will seek assistance where applicable. For data a provider controls independently, such as your Meta account, use that provider's privacy controls or request process. Deletion may be limited where retention is legally required.

## 6. Legal grounds and international processing

Where data-protection law requires a legal basis, necessary gameplay processing is used to provide the service you request. Support, troubleshooting, and security processing may rely on legitimate interests, subject to your rights. Processing may also be necessary to comply with legal obligations. Where consent is required, it must be obtained and can be withdrawn.

Service providers may process information in countries other than your own, including countries with different data-protection laws. Applicable provider agreements and legal transfer mechanisms govern such processing. Contact me for questions about processing for which I am responsible.

## 7. Children

ScornholeXR is intended for people aged 13 and older and is not directed to children under 13. Players must also meet the applicable Meta account and device age requirements in their region.

I do not knowingly collect personal information from children under 13. If you believe a child under 13 has provided personal information through the app or by contacting me, please email [bokijelaca@gmail.com](mailto:bokijelaca@gmail.com) so I can investigate and take appropriate steps, including deletion of information under my control where required.

## 8. Security

I aim to limit information processing to what is needed to operate and support the game. However, no application, network connection, or storage system can be guaranteed completely secure. Avoid including sensitive information in support messages or shared logs.

## 9. Changes to this policy

This policy may be updated when the app's features, service providers, or data practices change. The revised policy will show an updated effective date. Additional notice or consent will be provided where required by applicable law.

## 10. Contact

Developer: yellboy

App: ScornholeXR

Privacy and data-deletion requests: [bokijelaca@gmail.com](mailto:bokijelaca@gmail.com)
