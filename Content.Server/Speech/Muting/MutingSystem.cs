using Content.Server.Abilities.Mime;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Server.Speech.Components;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Puppet;
using Content.Shared.Speech;
using Content.Shared.Speech.Muting;
using Content.Shared.Bed.Sleep;

namespace Content.Server.Speech.Muting
{
    public sealed class MutingSystem : EntitySystem
    {
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MutedComponent, SpeakAttemptEvent>(OnSpeakAttempt);
            SubscribeLocalEvent<MutedComponent, EmoteEvent>(OnEmote, before: new[] { typeof(VocalSystem) });
            SubscribeLocalEvent<MutedComponent, ScreamActionEvent>(OnScreamAction, before: new[] { typeof(VocalSystem) });
            SubscribeLocalEvent<SleepingComponent, ScreamActionEvent>(OnScreamActionSleeping, before: new[] { typeof(VocalSystem) }); //ss220 fixSleep
        }

        private void OnEmote(EntityUid uid, MutedComponent component, ref EmoteEvent args)
        {
            if (args.Handled)
                return;

            //still leaves the text so it looks like they are pantomiming a laugh
            if (args.Emote.Category.HasFlag(EmoteCategory.Vocal))
                args.Handled = true;
        }

        private void OnScreamAction(EntityUid uid, MutedComponent component, ScreamActionEvent args)
        {
            if (args.Handled)
                return;

            if (HasComp<MimePowersComponent>(uid))
                _popupSystem.PopupEntity(Loc.GetString("mime-cant-speak"), uid, uid);

            else
                _popupSystem.PopupEntity(Loc.GetString("speech-muted"), uid, uid);
            args.Handled = true;
        }


        private void OnSpeakAttempt(EntityUid uid, MutedComponent component, SpeakAttemptEvent args)
        {
            // TODO something better than this.

            if (HasComp<MimePowersComponent>(uid))
                _popupSystem.PopupEntity(Loc.GetString("mime-cant-speak"), uid, uid);
            else if (HasComp<VentriloquistPuppetComponent>(uid))
                _popupSystem.PopupEntity(Loc.GetString("ventriloquist-puppet-cant-speak"), uid, uid);
            else
                _popupSystem.PopupEntity(Loc.GetString("speech-muted"), uid, uid);

            args.Cancel();
        }
        //ss220 fixSleep
        private void OnScreamActionSleeping(EntityUid uid, SleepingComponent component, ScreamActionEvent args)
        {
            if (args.Handled)
                return;
            _popupSystem.PopupEntity(Loc.GetString("Вы крепко спите."), uid, uid);
            args.Handled = true;
        }
        //ss220 fixSleep end
    }
}
