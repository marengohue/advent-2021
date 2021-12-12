use std::{borrow::Borrow, collections::{HashMap, HashSet}};

use itertools::Itertools;

mod input;

pub type Path = Vec<Cave>;

#[derive(Hash, Clone, PartialEq, Eq, Debug)]
pub enum Cave {
    Start,
    End,
    Small(String),
    Large(String),
}

#[derive(Debug)]
pub struct CaveSystem {
    caves: HashMap<Cave, HashSet<Cave>>
}

fn is_traversible(cave: &Cave, visited_small: &HashSet<Cave>) -> bool {
    match cave {
        // Can't go again through start
        &Cave::Start => false,
        // Only small caves that were not visited yet
        &Cave::Small(_) => !visited_small
            .iter()
            .any(|it| it.eq(cave)),
        // Big caves and finish are allowed
        _ => true
    }
}

fn enumerate_paths(cave_system: &CaveSystem, start_at: Cave, visited_small: HashSet<Cave>, can_double_enter: Option<Cave>) -> Vec<Path> {
    let connections= cave_system.caves[&start_at].borrow();

    connections
        .iter()
        .filter(|&it| is_traversible(it, &visited_small))
        .flat_map(|conn| {
            if let Cave::End = conn {
                return vec![vec![start_at.clone(), conn.clone()]];
            }
            if let Cave::Small(_) = conn {
                let (visited, next_double_entry) = match &can_double_enter {
                    Some(cde) if cde.eq(conn) => {
                        (visited_small.clone(), None)
                    },
                    _ => {
                        let mut visited_thus_far = visited_small.clone();
                        visited_thus_far.insert(conn.clone());
                        (visited_thus_far, can_double_enter.clone())
                    }
                };
                
                let mut all_subpaths = enumerate_paths(cave_system, conn.clone(), visited, next_double_entry);
                for path in all_subpaths.iter_mut() {
                    path.insert(0, start_at.clone());
                }
                return all_subpaths;
            }

            let mut all_subpaths = enumerate_paths(cave_system, conn.clone(), visited_small.clone(), can_double_enter.clone());
            for path in all_subpaths.iter_mut() {
                path.insert(0, start_at.clone());
            }
            all_subpaths
           
        })
        .collect_vec()
}

fn paths(cave_system: &CaveSystem) -> Vec<Path> {
    cave_system.caves.iter()
        .filter_map(|(cave, _)| {
            match cave {
                Cave::Small(_) => Some(cave.clone()),
                _ => None
            }
        })
        .flat_map(|small_double_enter| {
            enumerate_paths(cave_system, Cave::Start, HashSet::<Cave>::new(), Some(small_double_enter))
        })
        .unique()
        .collect::<Vec<_>>()
}

fn main() {
    let cave_system = input::read_input();
    println!("{}", paths(&cave_system).len());
}
