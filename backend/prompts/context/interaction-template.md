## User Message

The investor typed the following:

> {{user_message}}

{{#if conversationHistory}}
## Conversation History

Previous interactions this session (most recent last):

{{#each conversationHistory}}
**Investor:** {{this.message}}
**You responded with:** {{this.narrativeSummary}} (widgets: {{this.widgetTypes}})

{{/each}}
{{/if}}

Given this investor's profile, portfolio, current context signals, and their message above — decide what they need to see now. Return your response as JSON.
