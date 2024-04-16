← [README](README.md)

Sadly, I haven't even started on this yet. Please check out the sample content pack
for now to see what event data looks like. There are comments and sample events
you can trigger using the `mne_test` command.

Also, you can use Content Patcher to create events by targeting the resource:

```
Mods/leclair.morenightlyevents/Events
```


# Commands

## `mne_test [event]`

This allows you to force the next nightly event to be the event with
the given ID. This bypasses the conditions on the event, but the event
may fail to happen if other conditions aren't met (like a spawn event
not being able to spawn something).

You can use `mne_test clear` to clear the forced event.

## `mne_list`

List all the available events with their IDs.

## `mne_invalidate`

Invalidate the event cache, so it will be reloaded the next time an
event is called for. Use this to reload your event data files when
editing events for testing.


# Triggers

## `leclair.morenightlyevents_ForceEvent [event]`

Works like the `mne_test` command, but as a trigger action.
