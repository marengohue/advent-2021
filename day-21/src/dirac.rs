use crate::die::{Die, TossOutcome};

pub struct DiracDie {
    rolls: u64
}

const DIRAC_OUTCOMES: [TossOutcome; 7] = [
    TossOutcome { toss_sum: 3, outcome_count: 1 },
    TossOutcome { toss_sum: 4, outcome_count: 3 },
    TossOutcome { toss_sum: 5, outcome_count: 6 },
    TossOutcome { toss_sum: 6, outcome_count: 7 },
    TossOutcome { toss_sum: 7, outcome_count: 6 },
    TossOutcome { toss_sum: 8, outcome_count: 3 },
    TossOutcome { toss_sum: 9, outcome_count: 1 },
];

impl Die for DiracDie {
    fn roll_turn(&mut self) -> &[TossOutcome] {
        self.rolls += 3;
        &DIRAC_OUTCOMES
    }

    fn total_rolls(&self) -> u64 {
        self.rolls
    }
}

impl DiracDie {
    pub fn new() -> Self {
        DiracDie { rolls: 0 }
    }
}
