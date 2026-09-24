import { useState } from 'react';
import type { GroupType } from '../../types';
import { useGarden } from '../../state/GardenContext';
import { useCareDate } from '../../state/CareDateContext';
import { CheckIcon, PlusIcon } from '../../components/ui/icons';
import { CheckToggle } from '../../components/ui/CheckToggle';
import { CareDateBanner, CareDateChip } from '../../components/ui/CareDateBar';
import { AddGroupSheet } from './AddGroupSheet';
import { PlantPickerSheet } from './PlantPickerSheet';
import { selectGroups, type GroupAction } from './groups.selectors';
import styles from './GroupsPage.module.css';

type Sheet = { kind: 'picker'; groupId: string } | { kind: 'addGroup' } | null;

export const GroupsPage = () => {
  const garden = useGarden();
  const { careDate, isBackdated } = useCareDate();
  const [expanded, setExpanded] = useState<Record<string, boolean>>({});
  const [sheet, setSheet] = useState<Sheet>(null);

  const cards = selectGroups(garden.species, garden.plants, garden.groups, garden.today);
  const activeGroup =
    sheet?.kind === 'picker' ? garden.groups.find((g) => g.id === sheet.groupId) : undefined;

  const toggleExpand = (key: string) => setExpanded((cur) => ({ ...cur, [key]: !cur[key] }));

  const runAction = (action: GroupAction, ids: string[], message: string) =>
    void garden.commitAction(ids, action.type, message, careDate);

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <h1 className={styles.heading}>Grupy</h1>
        <div className={styles.headerActions}>
          <CareDateChip />
          <button type="button" className={styles.newBtn} onClick={() => setSheet({ kind: 'addGroup' })}>
            <PlusIcon size={14} />
            Nowa
          </button>
        </div>
      </header>

      <CareDateBanner />

      <div className={styles.list}>
        {cards.map((card) => (
          <article key={card.id} className={styles.card}>
            <div className={styles.cardHead}>
              <div className={styles.cardEmoji}>{card.emoji}</div>
              <div className={styles.cardText}>
                <div className={styles.cardName}>{card.name}</div>
                <div className={styles.cardMeta}>
                  <span className={styles.tag} style={{ background: card.tagBg, color: card.tagInk }}>
                    {card.typeLabel}
                  </span>
                  <span className={styles.memberSub}>{card.memberSub}</span>
                </div>
              </div>
            </div>

            {card.isRegion && (
              <div className={styles.regionStats}>
                <div className={styles.statWater}>
                  <div className={styles.statNum}>{card.regionWaterDue}</div>
                  <div className={styles.statLabel}>💧 do podlania</div>
                </div>
                <div className={styles.statFert}>
                  <div className={styles.statNum}>{card.regionFertDue}</div>
                  <div className={styles.statLabel}>🌱 do nawożenia</div>
                </div>
              </div>
            )}

            {card.showWarning && (
              <div className={styles.warning}>
                <span className={styles.warningIcon}>⚠️</span>
                <div>
                  <div className={styles.warningTitle}>Mieszane harmonogramy</div>
                  <div className={styles.warningText}>
                    Rośliny mają różny rytm — akcja zbiorcza rzadko obejmie wszystkie. Zwykle warto
                    rozbić grupę.
                  </div>
                </div>
              </div>
            )}

            {card.allClear && (
              <div className={styles.clear}>
                <CheckIcon size={18} />
                {card.clearLabel}
              </div>
            )}

            <div className={styles.actions}>
              {card.actions.map((action) => {
                const key = `${card.id}:${action.type}`;
                const isOpen = !!expanded[key];

                return (
                  <div key={action.type} className={styles.action}>
                    <div className={styles.actionHead}>
                      <div className={styles.actionTitle}>
                        <span className={styles.actionEmoji}>{action.emoji}</span>
                        <span>{action.label}</span>
                      </div>
                      <span className={styles.actionStat}>{action.headStat} dziś</span>
                    </div>

                    {action.none ? (
                      <div className={styles.actionNone}>
                        <CheckIcon size={15} />
                        Nic na dziś
                      </div>
                    ) : (
                      <div className={styles.actionButtons}>
                        <button
                          type="button"
                          className={styles.primaryBtn}
                          onClick={() =>
                            runAction(
                              action,
                              action.dueIds,
                              `${action.emoji} ${action.verb} potrzebujące (${action.due}) w „${card.name}”`,
                            )
                          }
                        >
                          {action.primaryBtn}
                        </button>
                        {action.partial && (
                          <button
                            type="button"
                            className={styles.allBtn}
                            onClick={() =>
                              runAction(
                                action,
                                action.allIds,
                                `${action.emoji} ${action.verb} wszystkie (${action.trackedCount}) w „${card.name}”`,
                              )
                            }
                          >
                            {action.allBtn}
                          </button>
                        )}
                      </div>
                    )}

                    <button type="button" className={styles.expandBtn} onClick={() => toggleExpand(key)}>
                      {isOpen ? 'Ukryj listę' : 'Które konkretnie?'}
                    </button>

                    {isOpen && (
                      <div className={styles.rows}>
                        {action.rows.map((row) => (
                          <div key={row.id} className={styles.row}>
                            <div className={styles.rowText}>
                              <div className={row.done ? styles.rowDone : styles.rowName}>{row.name}</div>
                              <div className={styles.rowSub}>{row.sub}</div>
                            </div>
                            <span className={styles.rowState}>{row.stateLabel}</span>
                            <CheckToggle
                              checked={row.done && !isBackdated}
                              disabled={row.done && !isBackdated}
                              onClick={() =>
                                runAction(action, [row.id], `${action.emoji} ${action.verb} ${row.name}`)
                              }
                              size={30}
                              radius={9}
                            />
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                );
              })}
            </div>

            <div className={styles.footer}>
              <button
                type="button"
                className={styles.footerBtn}
                onClick={() => setSheet({ kind: 'picker', groupId: card.id })}
              >
                <PlusIcon size={14} />
                Rośliny w grupie
              </button>
            </div>
          </article>
        ))}

        {!garden.isLoading && !cards.length && (
          <p className={styles.memberSub}>Nie masz jeszcze żadnych grup.</p>
        )}
      </div>

      <PlantPickerSheet
        open={sheet?.kind === 'picker'}
        onClose={() => setSheet(null)}
        group={activeGroup}
        plants={garden.plants}
        species={garden.species}
        groups={garden.groups}
        onToggle={(plantId, member) =>
          activeGroup && void garden.setGroupMembership(plantId, activeGroup.id, member)
        }
      />

      <AddGroupSheet
        open={sheet?.kind === 'addGroup'}
        onClose={() => setSheet(null)}
        onCreate={(name: string, type: GroupType) => {
          void garden.addGroup(name, type);
          setSheet(null);
        }}
      />
    </div>
  );
};
